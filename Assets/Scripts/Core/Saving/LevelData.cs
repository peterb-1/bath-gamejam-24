using System;
using Gameplay.Core;
using UnityEngine;
using Utils;

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
        
        [field: SerializeField]
        public bool IsBestTimePosted { get; private set; }

        [field: SerializeField]
        public byte[] GhostData { get; private set; }

        public LevelData(LevelConfig levelConfig)
        {
            LevelConfigGuid = levelConfig.Guid;
            BestTime = float.MaxValue;
            IsUnlocked = false;
            IsBestTimePosted = true;
            GhostData = null;
        }

        public bool TrySetTime(float time)
        {
            if (time < BestTime)
            {
                BestTime = time;
                IsBestTimePosted = false;
                return true;
            }

            return false;
        }

        public bool TryUnlock()
        {
            if (!IsUnlocked)
            {
                GameLogger.Log($"Unlocking level with config GUID {LevelConfigGuid}!");
                
                IsUnlocked = true;
                return true;
            }

            return false;
        }
        
        public void MarkAsPosted()
        {
            if (!IsComplete()) return;
            
            IsBestTimePosted = true;
        }
        
        public void SetGhostData(byte[] data)
        {
            GhostData = data;
        }

        public bool IsComplete()
        {
            return BestTime < float.MaxValue - 1e6f;
        }
    }
}