using Gameplay.Colour;
using TMPro;
using UnityEngine;

namespace UI
{
    public class WorldSpaceText : MonoBehaviour
    {
        [SerializeField] 
        private TMP_Text text;

        [SerializeField] 
        private ColourId colourId;

        [SerializeField] 
        private ColourDatabase colourDatabase;

        [SerializeField]
        private bool isVisibleOnAwake;

        private void Awake()
        {
            if (colourDatabase.TryGetColourConfig(colourId, out var colourConfig))
            {
                var currentColour = colourConfig.TextColour;
                currentColour.a = isVisibleOnAwake ? 1f : 0f;
                text.color = currentColour;
            }
            
        }

        public void ToggleVisibility()
        {
            var currentColour = text.color;
            currentColour.a = 1f - text.color.a;
            text.color = currentColour;
        }
    }
}