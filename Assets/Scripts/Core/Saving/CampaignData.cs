using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Saving
{
    [Serializable]
    public class CampaignData
    {
        [SerializeField] 
        private List<LevelData> levelDataEntries = new();

        public bool TryGetLevelData(SceneConfig sceneConfig, out LevelData levelData)
        {
            foreach (var data in levelDataEntries)
            {
                if (data.SceneConfig == sceneConfig)
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
                if (!TryGetLevelData(sceneConfig, out _) && sceneConfig.IsLevelScene)
                {
                    levelDataEntries.Add(new LevelData(sceneConfig));
                }
            }
        }
    }
}