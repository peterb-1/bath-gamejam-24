using Gameplay.Colour;
using UnityEngine;
using Utils;

namespace Gameplay.Player
{
    public class PlayerColourBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private SpriteRenderer playerSpriteRenderer;

        [SerializeField] 
        private ColourDatabase colourDatabase;
        
        private void Awake()
        {
            ColourManager.OnColourChangeStarted += HandleColourChangeStarted;
            ColourManager.OnColourChangeInstant += HandleColourChangeInstant;
        }

        private void HandleColourChangeInstant(ColourId colour)
        {
            if (colourDatabase.TryGetColourConfig(colour, out var colourConfig))
            {
                playerSpriteRenderer.color = colourConfig.PlayerColour;
            }
            else
            {
                GameLogger.LogError($"Cannot change player colour since the colour config for {colour} could not be found in the colour database!", colourDatabase);
            }
        }

        private void HandleColourChangeStarted(ColourId colour, float duration)
        {
            if (colourDatabase.TryGetColourConfig(colour, out var colourConfig))
            {
                playerSpriteRenderer.color = colourConfig.PlayerColour;
            }
            else
            {
                GameLogger.LogError($"Cannot change player colour since the colour config for {colour} could not be found in the colour database!", colourDatabase);
            }
        }

        private void OnDestroy()
        {
            ColourManager.OnColourChangeStarted -= HandleColourChangeStarted;
            ColourManager.OnColourChangeInstant -= HandleColourChangeInstant;
        }
    }
}
