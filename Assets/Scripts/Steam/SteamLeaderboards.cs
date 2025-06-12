using System;
using System.Collections.Generic;
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

                var success = await TryPostScoreAsync(leaderboard, levelData.BestTime);

                if (success)
                {
                    levelData.MarkAsPosted();
                    SaveManager.Instance.Save();
                }

                lastUploadTime = DateTime.UtcNow;
            }

            isUploading = false;
        }

        private async UniTask<bool> TryPostScoreAsync(SteamLeaderboard_t leaderboard, float timeSeconds)
        {
            var tcs = new UniTaskCompletionSource<bool>();
            var handle = SteamUserStats.UploadLeaderboardScore(
                leaderboard,
                ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest,
                timeSeconds.ToMilliseconds(),
                null,
                0
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

        public async UniTask<(bool success, List<float> scores)> TryGetGlobalScoresAsync(LevelConfig levelConfig, int maxEntries = 10)
        {
            if (!SteamManager.Initialized)
            {
                GameLogger.LogError($"Failed to get leaderboard scores for {levelConfig.GetSteamName()} as SteamManager is not initialised!", this);
                return (false, null);
            }

            if (!leaderboardLookup.TryGetValue(levelConfig, out var leaderboard))
            {
                GameLogger.Log($"Trying to get scores for {levelConfig.GetSteamName()} but did not find cached leaderboard - attempting fetch...", this);
                
                await TryGetLeaderboardFromSteamAsync(levelConfig);

                if (!leaderboardLookup.TryGetValue(levelConfig, out leaderboard))
                {
                    return (false, null);
                }
            }

            var tcs = new UniTaskCompletionSource<(bool, List<float>)>();
            var callResult = new CallResult<LeaderboardScoresDownloaded_t>();

            var handle = SteamUserStats.DownloadLeaderboardEntries(
                leaderboard,
                ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal,
                1,
                maxEntries
            );

            callResult.Set(handle, (result, bIOFailure) =>
            {
                if (bIOFailure || result.m_cEntryCount <= 0)
                {
                    GameLogger.LogError("Failed to download global leaderboard scores!");
                    tcs.TrySetResult((false, null));
                    return;
                }

                var entries = new List<float>();

                for (var i = 0; i < result.m_cEntryCount; i++)
                {
                    if (SteamUserStats.GetDownloadedLeaderboardEntry(
                            result.m_hSteamLeaderboardEntries, 
                            i,
                            out var entry, 
                            null, 
                            0))
                    {
                        entries.Add(entry.m_nScore.ToSeconds());
                    }
                }

                tcs.TrySetResult((true, entries));
            });

            return await tcs.Task;
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}