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

        private void Awake()
        {
            if (colourDatabase.TryGetColourConfig(colourId, out var colourConfig))
            {
                text.color = colourConfig.TextColour;
            }
        }
    }
}