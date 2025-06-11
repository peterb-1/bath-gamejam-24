using System;
using UnityEngine;

namespace Core.Saving
{
    [Serializable]
    public class AchievementData
    {
        [field: SerializeField]
        public string Guid { get; private set; }

        [field: SerializeField]
        public bool IsUnlocked { get; private set; }
        
        [field: SerializeField]
        public bool IsPosted { get; private set; }

        public AchievementData(string guid)
        {
            Guid = guid;
            IsUnlocked = false;
        }

        public bool TryUnlock()
        {
            if (IsUnlocked) return false;
            
            IsUnlocked = true;

            return true;
        }
        
        public void MarkAsPosted()
        {
            if (!IsUnlocked) return;
            
            IsPosted = true;
        }
        
#if UNITY_EDITOR
        public void Reset()
        {
            IsUnlocked = false;
            IsPosted = false;
        }
#endif
    }
}