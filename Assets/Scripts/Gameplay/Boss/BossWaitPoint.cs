using System;
using NaughtyAttributes;
using UnityEngine;

namespace Gameplay.Boss
{
    [Serializable]
    public struct BossWaitPoint
    {
        [field: SerializeField]
        public float Time { get; private set; }
        
        [field: SerializeField]
        public float CurveProgress { get; private set; }
        
        [field: SerializeField]
        public bool IsDamagePoint { get; private set; }
        
        [field: SerializeField, HideIf(nameof(IsDamagePoint)), AllowNesting]
        public Transform PlayerThreshold { get; private set; }
    }
}