using System;
using UnityEngine;

namespace Gameplay.Colour
{
    [Serializable]
    public class DistrictColourOverride
    {
        [field: SerializeField] 
        public int District { get; private set; }

        [field: SerializeField] 
        public ColourConfig ConfigOverride { get; private set; }
    }
}