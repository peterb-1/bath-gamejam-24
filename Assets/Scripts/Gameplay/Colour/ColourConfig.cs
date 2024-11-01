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
        public Color Background { get; private set; }

        public Color GetColour()
        {
            var lerp = Random.Range(0f, 1f);

            return (1f - lerp) * min + lerp * max;
        }
    }
}
