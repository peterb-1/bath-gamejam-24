using System;
using System.Collections.Generic;
using System.IO;
using Core;
using Core.Saving;
using Cysharp.Threading.Tasks;
using Gameplay.Core;
using Gameplay.Ghosts;
using Steamworks;
using UnityEngine;
using Utils;

namespace Steam
{
    public class SteamLeaderboards : MonoBehaviour
    {
        private const float UPLOAD_COOLDOWN = 60f;
        
        public int UploadsQueued => isUploading ? uploadQueue.Count : 0;
        public LevelConfig CurrentlyProcessedLevelConfig { get; private set; }
        public static SteamLeaderboards Instance { get; private set; }
        
        private readonly Dictionary<LevelConfig, SteamLeaderboard_t> leaderboardLookup = new();
        private readonly List<(LevelConfig, LevelData)> uploadQueue = new();
        
        private Callback<DownloadItemResult_t> downloadCallback;
        private UniTaskCompletionSource<GhostRun> ghostDownloadTcs;
        private PublishedFileId_t pendingDownloadFileId;
        private LevelConfig pendingDownloadLevelConfig;
        private DateTime lastUploadTime = DateTime.MinValue;
        private bool isUploading;

        private async void Awake()
        {
            if (Instance != null && Instance != this)
            {
                GameLogger.LogError("Cannot have more than one SteamLeaderboards in the scene at once! Destroying this one.");
                Destroy(this);
                return;
            }

            await GatherLeaderboardsAsync();

            Instance = this;
            
            HandleUnpostedScoresAsync().Forget();
        }
        
        public static bool IsReady() => Instance != null;

        private async UniTask GatherLeaderboardsAsync()
        {
            await UniTask.WaitUntil(() => SceneLoader.IsReady() && SteamManager.Initialized);

            foreach (var sceneConfig in SceneLoader.Instance.SceneConfigs)
            {
                if (sceneConfig.IsLevelScene)
                {
                    TryGetLeaderboardFromSteamAsync(sceneConfig.LevelConfig).Forget();
                }
            }
        }

        private async UniTask HandleUnpostedScoresAsync()
        {
            await UniTask.WaitUntil(() => SaveManager.IsReady);
            
            foreach (var sceneConfig in SceneLoader.Instance.SceneConfigs)
            {
                if (sceneConfig.IsLevelScene)
                {
                    QueueScoreUpload(sceneConfig.LevelConfig);
                }
            }
        }
        
        private async UniTask<bool> TryGetLeaderboardFromSteamAsync(LevelConfig config)
        {
            var tcs = new UniTaskCompletionSource<bool>();
            var handle = SteamUserStats.FindLeaderboard(config.GetSteamName());
            var callResult = new CallResult<LeaderboardFindResult_t>();
            
            callResult.Set(handle, (result, bIOFailure) =>
            {
                if (bIOFailure || result.m_bLeaderboardFound == 0)
                {
                    GameLogger.LogError($"Failed to find leaderboard for {config.GetSteamName()}", this);
                    tcs.TrySetResult(false);
                }
                else
                {
                    GameLogger.Log($"Found leaderboard for {config.GetSteamName()}", this);
                    leaderboardLookup[config] = result.m_hSteamLeaderboard;
                    tcs.TrySetResult(true);
                }
            });

            return await tcs.Task;
        }
        
        private async UniTask<(bool, SteamLeaderboard_t)> TryGetLeaderboardAsync(LevelConfig levelConfig)
        {
            if (!SteamManager.Initialized)
            {
                GameLogger.LogError($"Failed to get leaderboard for {levelConfig.GetSteamName()} to Steam as SteamManager is not initialised!", this);
                return (false, new SteamLeaderboard_t());
            }

            if (!leaderboardLookup.TryGetValue(levelConfig, out var leaderboard))
            {
                GameLogger.Log($"Failed to find cached leaderboard {levelConfig.GetSteamName()} - attempting fetch...", this);
                
                await TryGetLeaderboardFromSteamAsync(levelConfig);

                if (!leaderboardLookup.TryGetValue(levelConfig, out leaderboard))
                {
                    return (false, new SteamLeaderboard_t());
                }
            }

            return (true, leaderboard);
        }
        
        public void QueueScoreUpload(LevelConfig levelConfig)
        {
            foreach (var (config, _) in uploadQueue)
            {
                if (config.Guid == levelConfig.Guid) return;
            }
            
            if (!SaveManager.Instance.SaveData.CampaignData.TryGetLevelData(levelConfig, out var levelData)) return;
            if (!levelData.IsComplete() || levelData.IsBestTimePosted) return;
            
            GameLogger.Log($"Queueing score upload for {levelConfig.GetSteamName()}...", this);
            
            uploadQueue.Add((levelConfig, levelData));
                
            if (!isUploading)
            {
                ProcessUploadQueueAsync().Forget();
            }
        }

        private async UniTask ProcessUploadQueueAsync()
        {
            isUploading = true;

            while (uploadQueue.Count > 0)
            {
                var timeSincePreviousUpload = (DateTime.UtcNow - lastUploadTime).TotalSeconds;
                
                if (timeSincePreviousUpload < UPLOAD_COOLDOWN)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(UPLOAD_COOLDOWN - timeSincePreviousUpload));
                }
                
                // if this fails, it will return default value (most recent)
                SaveManager.Instance.SaveData.PreferenceData.TryGetValue(SettingId.LeaderboardPriority, out LeaderboardPriority priority);
                
                var index = priority is LeaderboardPriority.Newest ? uploadQueue.Count - 1 : 0;
                var (levelConfig, levelData) = uploadQueue[index];
                
                CurrentlyProcessedLevelConfig = levelConfig;
                uploadQueue.RemoveAt(index);
                
                GameLogger.Log($"Processing score upload for {levelConfig.GetSteamName()}...", this);

                var (foundLeaderboard, leaderboard) = await TryGetLeaderboardAsync(levelConfig);

                if (!foundLeaderboard)
                {
                    GameLogger.LogError($"Failed to upload score for {levelConfig.GetSteamName()} as leaderboard could not be found!", this);
                    continue;
                }

                var ghostFileId = await UploadGhostDataAsync(levelConfig, levelData.GhostData);

                if (!ghostFileId.HasValue)
                {
                    GameLogger.LogError($"Failed to upload ghost data as UGC for {levelConfig.GetSteamName()}!", this);
                    continue;
                }
                
                var fileId = ghostFileId.Value.m_PublishedFileId;
                var lowerBits = (int) (fileId & 0xFFFFFFFF);
                var upperBits = (int) ((fileId >> 32) & 0xFFFFFFFF);
                var details = new[] { lowerBits, upperBits };
                
                var success = await TryPostScoreAsync(leaderboard, levelData.BestMilliseconds, details);

                if (success)
                {
                    levelData.MarkAsPosted();
                    SaveManager.Instance.Save();
                }

                lastUploadTime = DateTime.UtcNow;
                CurrentlyProcessedLevelConfig = null;
            }

            isUploading = false;
        }

        private async UniTask<bool> TryPostScoreAsync(SteamLeaderboard_t leaderboard, int milliseconds, int[] details)
        {
            var tcs = new UniTaskCompletionSource<bool>();
            var handle = SteamUserStats.UploadLeaderboardScore(
                leaderboard,
                ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest,
                milliseconds,
                details,
                details.Length
            );

            var uploadResult = new CallResult<LeaderboardScoreUploaded_t>();
            
            uploadResult.Set(handle, (result, bIOFailure) =>
            {
                if (bIOFailure || result.m_bSuccess == 0)
                {
                    GameLogger.LogError("Failed to upload score!", this);
                    tcs.TrySetResult(false);
                }
                else
                {
                    GameLogger.Log($"Uploaded score {result.m_nScore} to leaderboard {result.m_hSteamLeaderboard}", this);
                    tcs.TrySetResult(true);
                }
            });

            return await tcs.Task;
        }
        
        private async UniTask<PublishedFileId_t?> UploadGhostDataAsync(LevelConfig levelConfig, string ghostData)
        {
            try
            {
                var tempDirectory = Path.Combine(Path.GetTempPath(), levelConfig.GetSteamGhostFileName()).Replace('\\', '/');
                var tempPath = Path.Combine(tempDirectory, levelConfig.GetSteamGhostFileName());
                
                Directory.CreateDirectory(tempDirectory);

                await File.WriteAllTextAsync(tempPath, ghostData);

                var tcs = new UniTaskCompletionSource<PublishedFileId_t?>();
                var createHandle = SteamUGC.CreateItem(SteamUtils.GetAppID(), EWorkshopFileType.k_EWorkshopFileTypeGameManagedItem);
                var createCallResult = new CallResult<CreateItemResult_t>();
                
                createCallResult.Set(createHandle, (createResult, bIOFailure) =>
                {
                    if (bIOFailure || createResult.m_eResult != EResult.k_EResultOK)
                    {
                        GameLogger.LogError($"Failed to create UGC item for {levelConfig.GetSteamName()}: {createResult.m_eResult}", this);
                        tcs.TrySetResult(null);
                        return;
                    }

                    var updateHandle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), createResult.m_nPublishedFileId);
                    
                    // steam fucking crashes the entire game if you don't do all of these things which are mentioned nowhere in the documentation
                    SteamUGC.SetItemTitle(updateHandle, levelConfig.GetSteamGhostFileName());
                    SteamUGC.SetItemDescription(updateHandle, $"Ghost data for {levelConfig.GetSteamName()}");
                    SteamUGC.SetItemContent(updateHandle, tempDirectory);
                    SteamUGC.SetItemVisibility(updateHandle, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityUnlisted);
                    SteamUGC.SetItemPreview(updateHandle, string.Empty);
                    SteamUGC.SetItemTags(updateHandle, new List<string>());

                    var submitHandle = SteamUGC.SubmitItemUpdate(updateHandle, $"Ghost data for {levelConfig.GetSteamName()}");
                    var submitCallResult = new CallResult<SubmitItemUpdateResult_t>();
                    
                    submitCallResult.Set(submitHandle, (submitResult, bIOFailure2) =>
                    {
                        try
                        {
                            if (Directory.Exists(tempDirectory))
                            {
                                Directory.Delete(tempDirectory, true);
                            }
                        }
                        catch
                        {
                            // doesn't matter
                        }

                        if (bIOFailure2 || submitResult.m_eResult != EResult.k_EResultOK)
                        {
                            GameLogger.LogError($"Failed to upload ghost data for {levelConfig.GetSteamName()}: {submitResult.m_eResult}", this);
                            tcs.TrySetResult(null);
                        }
                        else
                        {
                            GameLogger.Log($"Successfully uploaded ghost data for {levelConfig.GetSteamName()} with file ID {createResult.m_nPublishedFileId}", this);
                            tcs.TrySetResult(createResult.m_nPublishedFileId);
                        }
                    });
                });

                return await tcs.Task;
            }
            catch (Exception e)
            {
                GameLogger.LogError($"Failed to upload ghost data due to exception: {e}", this);
                return null;
            }
        }

        public async UniTask<(LeaderboardResultStatus, List<LeaderboardResult>)> TryGetLeaderboardScoresAsync(
            LevelConfig levelConfig, 
            ELeaderboardDataRequest requestType, 
            int start,
            int end)
        {
            if (!SteamManager.Initialized)
            {
                GameLogger.LogError($"Failed to get leaderboard scores for {levelConfig.GetSteamName()} as SteamManager is not initialised!", this);
                return (LeaderboardResultStatus.Failure, null);
            }

            if (!leaderboardLookup.TryGetValue(levelConfig, out var leaderboard))
            {
                GameLogger.Log($"Trying to get scores for {levelConfig.GetSteamName()} but did not find cached leaderboard - attempting fetch...", this);
                
                await TryGetLeaderboardFromSteamAsync(levelConfig);

                if (!leaderboardLookup.TryGetValue(levelConfig, out leaderboard))
                {
                    return (LeaderboardResultStatus.Failure, null);
                }
            }

            var tcs = new UniTaskCompletionSource<(LeaderboardResultStatus, List<LeaderboardResult>)>();
            var callResult = new CallResult<LeaderboardScoresDownloaded_t>();

            var handle = SteamUserStats.DownloadLeaderboardEntries(
                leaderboard,
                requestType,
                start,
                end
            );

            callResult.Set(handle, (result, bIOFailure) =>
            {
                if (bIOFailure || result.m_cEntryCount < 0)
                {
                    GameLogger.LogError("Failed to download global leaderboard scores!");
                    tcs.TrySetResult((LeaderboardResultStatus.Failure, null));
                    return;
                }

                if (result.m_cEntryCount <= 0)
                {
                    GameLogger.Log("Downloaded global leaderboard scores, but found no entries");
                    tcs.TrySetResult((LeaderboardResultStatus.NoEntries, null));
                    return;
                }

                var entries = new List<LeaderboardResult>();

                for (var i = 0; i < result.m_cEntryCount; i++)
                {
                    var detailsBuffer = new int[2];
                    
                    if (SteamUserStats.GetDownloadedLeaderboardEntry(result.m_hSteamLeaderboardEntries, i, out var entry, detailsBuffer, 2))
                    {
                        entries.Add(new LeaderboardResult(entry, detailsBuffer));
                    }
                }

                tcs.TrySetResult((LeaderboardResultStatus.FoundEntries, entries));
            });

            return await tcs.Task;
        }

        public async UniTask<GhostRun> TryGetGhostDataAsync(LevelConfig levelConfig, ulong fileId)
        {
            var ghostFileId = new PublishedFileId_t(fileId);
            var alreadyDownloadedData = TryGetDownloadedGhostData(levelConfig, ghostFileId);

            if (alreadyDownloadedData != null)
            {
                return alreadyDownloadedData;
            }
            
            if (ghostDownloadTcs != null)
            {
                GameLogger.LogError("Another ghost download is already in progress!");
                return null;
            }

            ghostDownloadTcs = new UniTaskCompletionSource<GhostRun>();
            pendingDownloadFileId = ghostFileId;
            pendingDownloadLevelConfig = levelConfig;
            downloadCallback ??= Callback<DownloadItemResult_t>.Create(OnGhostDownloadCompleted);

            if (!SteamUGC.DownloadItem(ghostFileId, true))
            {
                GameLogger.LogError("Failed to start downloading ghost data!");
                return null;
            }

            return await ghostDownloadTcs.Task;
        }

        public GhostRun TryGetOfflineGhostData(LevelConfig levelConfig, ulong fileId)
        {
            var ghostFileId = new PublishedFileId_t(fileId);
            return TryGetDownloadedGhostData(levelConfig, ghostFileId);
        }

        private GhostRun TryGetDownloadedGhostData(LevelConfig levelConfig, PublishedFileId_t fileId)
        {
            if (!SteamUGC.GetItemInstallInfo(fileId, out _, out var folderPath, 1024, out _))
            {
                GameLogger.LogError($"Failed to get install info for item {fileId}!");
                return null;
            }
            
            try
            {
                var ghostFilePath = Path.Combine(folderPath, levelConfig.GetSteamGhostFileName());
                
                if (!File.Exists(ghostFilePath))
                {
                    ghostFilePath = null;
                    
                    foreach (var file in Directory.EnumerateFiles(folderPath))
                    {
                        ghostFilePath = file;
                        break;
                    }
                }

                if (ghostFilePath == null || !File.Exists(ghostFilePath))
                {
                    GameLogger.LogError("Ghost file not found in installed UGC directory.");
                    return null;
                }

                var ghostData = File.ReadAllText(ghostFilePath);
                
                GameLogger.Log($"Got ghost data from path {ghostFilePath}!");

                return GhostCompressor.Deserialize(ghostData);
            }
            catch (Exception e)
            {
                GameLogger.LogError($"Exception reading ghost file: {e}", this);
                return null;
            }
        }

        private void OnGhostDownloadCompleted(DownloadItemResult_t result)
        {
            if (result.m_nPublishedFileId != pendingDownloadFileId) return;

            if (result.m_eResult != EResult.k_EResultOK)
            {
                GameLogger.LogError($"Failed download with result {result.m_eResult}");
                ghostDownloadTcs?.TrySetResult(null);
                return;
            }

            var ghostResult = TryGetDownloadedGhostData(pendingDownloadLevelConfig, result.m_nPublishedFileId);

            ghostDownloadTcs?.TrySetResult(ghostResult);

            pendingDownloadFileId = default;
            pendingDownloadLevelConfig = null;
            ghostDownloadTcs = null;
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}