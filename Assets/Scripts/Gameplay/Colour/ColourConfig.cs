using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.Colour
{
    [Serializable]
    public class ColourConfig
    {
        [field: SerializeField] 
        public ColourId ColourId { get; private set; }

        [SerializeField] 
        private Color min;
        
        [SerializeField] 
        private Color max;
        
        [SerializeField] 
        private Color minDesaturated;
        
        [SerializeField] 
        private Color maxDesaturated;
        
        [field: SerializeField]
        public Color PlayerColour { get; private set; }
        
        [field: SerializeField]
        public Color DroneColour { get; private set; }
        
        [field: SerializeField]
        public Color SpringColour { get; private set; }
        
        [field: SerializeField]
        public Color[] DistrictPlatformColours { get; private set; }
        
        [field: SerializeField]
        public Color Background { get; private set; }
        
        [field: SerializeField]
        public Color BackgroundDesaturated { get; private set; }
        
        [field: SerializeField]
        public Color TextColour { get; private set; }
        
        [field: SerializeField]
        public Gradient ZiplineGradient { get; private set; }

        public Color GetRandomColour()
        {
            var lerp = Random.Range(0f, 1f);

            return (1f - lerp) * min + lerp * max;
        }
        
        public Color GetRandomDesaturatedColour()
        {
            var lerp = Random.Range(0f, 1f);

            return (1f - lerp) * minDesaturated + lerp * maxDesaturated;
        }
    }
}
