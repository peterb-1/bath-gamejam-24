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

        public async UniTask InitialiseAsync()
        {
            await UniTask.WaitUntil(SceneLoader.IsReady);

            foreach (var sceneConfig in SceneLoader.Instance.SceneConfigs)
            {
                if (!sceneConfig.IsLevelScene) continue;

                var levelConfig = sceneConfig.LevelConfig;
                
                if (!TryGetLevelData(levelConfig, out _))
                {
                    var newLevelData = new LevelData(levelConfig);

                    if (levelConfig.IsUnlockedByDefault)
                    {
                        newLevelData.TryUnlock();
                    }
                    
                    levelDataEntries.Add(newLevelData);
                }
            }
        }
    }
}