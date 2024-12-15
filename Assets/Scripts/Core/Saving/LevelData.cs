using System;
using Gameplay.Core;
using UnityEngine;

namespace Core.Saving
{
    [Serializable]
    public class LevelData
    {
        [field: SerializeField] 
        public string LevelConfigGuid { get; private set; }
        
        [field: SerializeField]
        public float BestTime { get; private set; }
        
        [field: SerializeField]
        public bool IsUnlocked { get; private set; }

        public LevelData(LevelConfig levelConfig)
        {
            LevelConfigGuid = levelConfig.Guid;
            BestTime = float.MaxValue;
            IsUnlocked = false;
        }

        public bool TrySetTime(float time)
        {
            if (time < BestTime)
            {
                BestTime = time;
                return true;
            }

            return false;
        }

        public bool TryUnlock()
        {
            if (!IsUnlocked)
            {
                IsUnlocked = true;
                return true;
            }

            return false;
        }
    }
}