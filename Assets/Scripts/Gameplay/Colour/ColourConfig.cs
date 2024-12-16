using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.Colour
{
    [Serializable]
    public class ColourConfig
    {
        [SerializeField] 
        public ColourId colourId;

        [SerializeField] 
        private Color min;
        
        [SerializeField] 
        private Color max;
        
        [field: SerializeField]
        public Color PlayerColour { get; private set; }
        
        [field: SerializeField]
        public Color DroneColour { get; private set; }
        
        [field: SerializeField]
        public Color PlatformColour { get; private set; }
        
        [field: SerializeField]
        public Color Background { get; private set; }

        public Color GetRandomColour()
        {
            var lerp = Random.Range(0f, 1f);

            return (1f - lerp) * min + lerp * max;
        }
    }
}
