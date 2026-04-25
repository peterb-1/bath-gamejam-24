using System;
using UnityEngine;

namespace Gameplay.Camera
{
    [Serializable]
    public class ShakeConfig
    {
        [field: SerializeField] 
        public float Duration { get; private set; }
        
        [field: SerializeField] 
        public AnimationCurve Shape { get; private set; }
        
        [field: SerializeField] 
        public float Strength { get; private set; }
    }
}