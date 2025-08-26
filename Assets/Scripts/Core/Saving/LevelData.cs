using System;
using System.Collections.Generic;
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
        public int BestMilliseconds { get; private set; }
        
        [field: SerializeField]
        public List<ushort> DronesKilled { get; private set; }
        
        [field: SerializeField]
        public bool HasFoundCollectible { get; private set; }
        
        [field: SerializeField]
        public bool HasShownUnlockAnimation { get; private set; }

        [field: SerializeField]
        public bool IsUnlocked { get; private set; }
        
        [field: SerializeField]
        public bool IsBestTimePosted { get; private set; }

        [field: SerializeField]
        public string GhostData { get; private set; }

        public event Action OnNewBestAchieved;

        public LevelData(LevelConfig levelConfig)
        {
            LevelConfigGuid = levelConfig.Guid;
            BestMilliseconds = int.MaxValue;
            DronesKilled = new List<ushort>();
            HasFoundCollectible = false;
            HasShownUnlockAnimation = false;
            IsUnlocked = false;
            IsBestTimePosted = true;
            GhostData = null;
        }

        public bool TrySetTime(int milliseconds)
        {
            if (milliseconds < BestMilliseconds)
            {
                BestMilliseconds = milliseconds;
                IsBestTimePosted = false;
                
                OnNewBestAchieved?.Invoke();
                
                return true;
            }

            return false;
        }

        public bool TryNotifyDroneKilled(ushort droneId)
        {
            if (!DronesKilled.Contains(droneId))
            {
                DronesKilled.Add(droneId);

                return true;
            }

            return false;
        }

        public bool TryMarkCollectibleAsFound()
        {
            if (!HasFoundCollectible)
            {
                HasFoundCollectible = true;
                return true;
            }

            return false;
        }
        
        public void MarkUnlockAnimationAsShown()
        {
            HasShownUnlockAnimation = true;
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
        
        public void SetGhostData(string data)
        {
            GhostData = data;
        }

        public bool IsComplete()
        {
            return BestMilliseconds < int.MaxValue;
        }
    }
}