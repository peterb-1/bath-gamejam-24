using System;
using UnityEngine;

namespace Core.Saving
{
    [Serializable]
    public class LevelData
    {
        [field: SerializeField] 
        public SceneConfig SceneConfig { get; private set; }
        
        [field: SerializeField]
        public float BestTime { get; private set; }

        public LevelData(SceneConfig sceneConfig)
        {
            SceneConfig = sceneConfig;
            BestTime = float.MaxValue;
        }

        public void SetTime(float time)
        {
            if (time < BestTime)
            {
                BestTime = time;
            }
        }
    }
}