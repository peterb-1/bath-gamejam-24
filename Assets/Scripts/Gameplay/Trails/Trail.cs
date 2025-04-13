using System;
using NaughtyAttributes;
using UnityEngine;

namespace Gameplay.Trails
{
    [Serializable]
    public class Trail
    {
        [field: SerializeField]
        public string Name { get; private set; }
        
        [field: SerializeField, ReadOnly, AllowNesting] 
        public string Guid { get; private set; }
        
        [field: SerializeField]
        public bool IsUnlockedByDefault { get; private set; }
        
        [field: SerializeField]
        public TrailRenderer TrailRenderer { get; private set; }
        
#if UNITY_EDITOR
        public void SetGuid(Guid guid)
        {
            Guid = guid.ToString();
        }
#endif
    }
}