using System;
using System.Collections.Generic;
using System.IO;
using Core;
using Core.Saving;
using Cysharp.Threading.Tasks;
using Gameplay.Core;
using Steamworks;
using UnityEngine;
using Utils;

namespace Steam
{
    public class SteamLeaderboards : MonoBehaviour
    {
        private const float UPLOAD_COOLDOWN = 60f;
        
        public static SteamLeaderboards Instance { get; private set; }
        
        private readonly Dictionary<LevelConfig, SteamLeaderboard_t> leaderboardLookup = new();
        private readonly Queue<(LevelConfig, LevelData)> uploadQueue = new();
        
        private Callback<DownloadItemResult_t> downloadCallback;
        private UniTaskCompletionSource<string> ghostDownloadTcs;
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
            
            uploadQueue.Enqueue((levelConfig, levelData));
                
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
                var (levelConfig, levelData) = uploadQueue.Dequeue();
                var timeSincePreviousUpload = (DateTime.UtcNow - lastUploadTime).TotalSeconds;
                
                GameLogger.Log($"Processing score upload for {levelConfig.GetSteamName()}...", this);
                
                if (timeSincePreviousUpload < UPLOAD_COOLDOWN)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(UPLOAD_COOLDOWN - timeSincePreviousUpload));
                }

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
                
                var success = await TryPostScoreAsync(leaderboard, levelData.BestTime, details);

                if (success)
                {
                    levelData.MarkAsPosted();
                    SaveManager.Instance.Save();
                }

                lastUploadTime = DateTime.UtcNow;
            }

            isUploading = false;
        }

        private async UniTask<bool> TryPostScoreAsync(SteamLeaderboard_t leaderboard, float timeSeconds, int[] details)
        {
            var tcs = new UniTaskCompletionSource<bool>();
            var handle = SteamUserStats.UploadLeaderboardScore(
                leaderboard,
                ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest,
                timeSeconds.ToMilliseconds(),
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

        public async UniTask<(LeaderboardResultStatus, List<LeaderboardResult>)> TryGetGlobalScoresAsync(LevelConfig levelConfig, int maxEntries = 10)
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
                ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal,
                1,
                maxEntries
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

        public async UniTask<string> TryGetGhostDataAsync(LevelConfig levelConfig, int[] details)
        {
            var fileId = new PublishedFileId_t(((ulong)details[1] << 32) | (uint)details[0]);

            ghostDownloadTcs = new UniTaskCompletionSource<string>();
            pendingDownloadFileId = fileId;
            pendingDownloadLevelConfig = levelConfig;
            downloadCallback ??= Callback<DownloadItemResult_t>.Create(OnGhostDownloadCompleted);

            if (!SteamUGC.DownloadItem(fileId, true))
            {
                GameLogger.LogError("Failed to start downloading ghost data!");
                return null;
            }

            return await ghostDownloadTcs.Task;
        }

        private void OnGhostDownloadCompleted(DownloadItemResult_t result)
        {
            GameLogger.Log($"Got download complete callback with result {result.m_eResult}");
            
            if (result.m_nPublishedFileId != pendingDownloadFileId) return;

            if (result.m_eResult != EResult.k_EResultOK)
            {
                GameLogger.LogError($"DownloadItemResult failed: {result.m_eResult}");
                ghostDownloadTcs?.TrySetResult(null);
                return;
            }

            if (!SteamUGC.GetItemInstallInfo(result.m_nPublishedFileId, out _, out var folderPath, 1024, out _))
            {
                GameLogger.LogError("Failed to get install info after ghost download.");
                ghostDownloadTcs?.TrySetResult(null);
                return;
            }

            try
            {
                var ghostFilePath = Path.Combine(folderPath, pendingDownloadLevelConfig.GetSteamGhostFileName());
                
                if (!File.Exists(ghostFilePath))
                {
                    GameLogger.Log($"Fallback to first file in {folderPath} for ghost data...");
                    
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
                    ghostDownloadTcs?.TrySetResult(null);
                    return;
                }

                var ghostData = File.ReadAllText(ghostFilePath);
                
                GameLogger.Log($"Got ghost data from path {ghostFilePath}!");
                
                ghostDownloadTcs?.TrySetResult(ghostData);
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Exception reading ghost file: {ex}", this);
                ghostDownloadTcs?.TrySetResult(null);
            }
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}