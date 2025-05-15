using System;
using UnityEngine;

namespace Hardware
{
    [Serializable]
    public class ContinuousRumbleConfig
    {
        [field: SerializeField] 
        public float LowFrequency { get; private set; }
        
        [field: SerializeField] 
        public float HighFrequency { get; private set; }
    }
}