using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Core;
using UnityEngine;

namespace Core.Saving
{
    [Serializable]
    public class CampaignData
    {
        [SerializeField] 
        private List<LevelData> levelDataEntries = new();

        public event Action OnNewBestAchieved;
        public event Action<int> OnRainbowRanksAchieved;
        
        public async UniTask InitialiseAsync()
        {
            await UniTask.WaitUntil(SceneLoader.IsReady);

            foreach (var sceneConfig in SceneLoader.Instance.SceneConfigs)
            {
                if (!sceneConfig.IsLevelScene) continue;

                var levelConfig = sceneConfig.LevelConfig;
                
                if (!TryGetLevelData(levelConfig, out var levelData))
                {
                    levelData = new LevelData(levelConfig);

                    if (levelConfig.IsUnlockedByDefault)
                    {
                        levelData.TryUnlock();
                    }
                    
                    levelDataEntries.Add(levelData);
                }
                
                levelData.OnNewBestAchieved += HandleNewBestAchieved;
            }
        }

        public bool TryGetLevelData(LevelConfig levelConfig, out LevelData levelData)
        {
            foreach (var data in levelDataEntries)
            {
                if (data.LevelConfigGuid == levelConfig.Guid)
                {
                    levelData = data;
                    return true;
                }
            }

            levelData = null;
            return false;
        }
        
        public bool TryMarkCollectibleAsFound(LevelConfig collectibleLevelConfig)
        {
            if (TryGetLevelData(collectibleLevelConfig, out var collectibleLevelData))
            {
                if (!collectibleLevelData.TryMarkCollectibleAsFound())
                {
                    return false;
                }
            }

            var areAllDistrictCollectiblesUnlocked = true;
            LevelConfig hiddenLevelConfig = null;

            foreach (var sceneConfig in SceneLoader.Instance.SceneConfigs)
            {
                if (!sceneConfig.IsLevelScene) continue;

                var levelConfig = sceneConfig.LevelConfig;
                
                if (levelConfig.DistrictNumber != collectibleLevelConfig.DistrictNumber) continue;

                if (levelConfig.IsHidden)
                {
                    hiddenLevelConfig = levelConfig;
                    continue;
                }

                if (levelConfig.HasCollectible && TryGetLevelData(levelConfig, out var levelData))
                {
                    areAllDistrictCollectiblesUnlocked &= levelData.HasFoundCollectible;
                }
            }

            if (areAllDistrictCollectiblesUnlocked && hiddenLevelConfig != null && TryGetLevelData(hiddenLevelConfig, out var hiddenLevelData))
            {
                return hiddenLevelData.TryUnlock();
            }
            
            return false;
        }

        public (int completed, int stars, int totalMissions) GetDistrictProgress(int district)
        {
            var completed = 0;
            var stars = 0;
            var totalMissions = 0;
            
            foreach (var sceneConfig in SceneLoader.Instance.SceneConfigs)
            {
                if (!sceneConfig.IsLevelScene) continue;

                var levelConfig = sceneConfig.LevelConfig;
                
                if (levelConfig.DistrictNumber != district) continue;

                if (TryGetLevelData(levelConfig, out var levelData))
                {
                    totalMissions++;
                    stars += levelConfig.GetStars(levelData.BestTime);

                    if (levelData.IsComplete())
                    {
                        completed++;
                    }
                }
            }

            return (completed, stars, totalMissions);
        }

        private void HandleNewBestAchieved()
        {
            var rainbowRanksAchieved = 0;
            var allStarDistricts = new List<int>();
            
            foreach (var sceneConfig in SceneLoader.Instance.SceneConfigs)
            {
                if (!sceneConfig.IsLevelScene) continue;

                var levelConfig = sceneConfig.LevelConfig;
                
                if (TryGetLevelData(levelConfig, out var levelData) && levelConfig.Guid == levelData.LevelConfigGuid)
                {
                    if (levelConfig.GetTimeRanking(levelData.BestTime) is TimeRanking.Rainbow)
                    {
                        rainbowRanksAchieved++;
                    }
                }
            }
            
            OnNewBestAchieved?.Invoke();
            OnRainbowRanksAchieved?.Invoke(rainbowRanksAchieved);
        }
    }
}