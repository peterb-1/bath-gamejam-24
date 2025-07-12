using System;
using NaughtyAttributes;
using UnityEngine;

namespace Gameplay.Boss
{
    [Serializable]
    public struct BossWaitPoint
    {
        public enum BossProgressType
        {
            Timed,
            Event,
            Damage
        }
        
        [field: SerializeField]
        public float Time { get; private set; }
        
        [field: SerializeField]
        public BossMovementCurve Curve { get; private set; }
        
        [field: SerializeField]
        public BossProgressType ProgressType { get; private set; }
        
        [field: SerializeField, ShowIf(nameof(ProgressType), BossProgressType.Timed), AllowNesting]
        public float TimeToProgress { get; private set; }

        public BossWaitPoint(BossMovementCurve curve)
        {
            Curve = curve;
            ProgressType = BossProgressType.Timed;
            Time = 1;
            TimeToProgress = 0;
        }
    }
}