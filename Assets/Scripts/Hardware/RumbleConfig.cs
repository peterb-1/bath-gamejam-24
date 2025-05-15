using System;
using UnityEngine;

namespace Hardware
{
    [Serializable]
    public class RumbleConfig
    {
        [field: SerializeField] 
        public float Duration { get; private set; }
        
        [field: SerializeField] 
        public AnimationCurve LowFrequencyCurve { get; private set; }
        
        [field: SerializeField] 
        public AnimationCurve HighFrequencyCurve { get; private set; }
    }
}