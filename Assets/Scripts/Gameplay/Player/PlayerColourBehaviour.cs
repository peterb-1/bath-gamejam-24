using Cysharp.Threading.Tasks;
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
        private ParticleSystemRenderer deathParticleRenderer;

        [SerializeField] 
        private ColourDatabase colourDatabase;

        [SerializeField] 
        private AnimationCurve flashCurve;
        
        private void Awake()
        {
            ColourManager.OnColourChangeStarted += HandleColourChangeStarted;
            ColourManager.OnColourChangeInstant += HandleColourChangeInstant;
        }

        private void HandleColourChangeInstant(ColourId colour)
        {
            if (colourDatabase.TryGetColourConfig(colour, out var colourConfig))
            {
                SetColour(colourConfig);
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
                SetColour(colourConfig);
                RunFlashAsync(duration).Forget();
            }
            else
            {
                GameLogger.LogError($"Cannot change player colour since the colour config for {colour} could not be found in the colour database!", colourDatabase);
            }
        }

        private void SetColour(ColourConfig colourConfig)
        {
            playerSpriteRenderer.color = colourConfig.PlayerColour;
            deathParticleRenderer.material.color = colourConfig.PlayerColour;
        }

        private async UniTask RunFlashAsync(float duration)
        {
            var initialTime = Time.realtimeSinceStartup;
            var startColour = playerSpriteRenderer.color;
            var timeElapsed = 0f;

            // run flash independent of timescale, since this happens during the slowdown
            while (timeElapsed < duration)
            {
                var lerp = flashCurve.Evaluate(timeElapsed / duration);

                playerSpriteRenderer.color = (1f - lerp) * startColour + lerp * Color.white;
                
                await UniTask.Yield();
                
                timeElapsed = Time.realtimeSinceStartup - initialTime;
            }

            playerSpriteRenderer.color = startColour;
        }

        private void OnDestroy()
        {
            ColourManager.OnColourChangeStarted -= HandleColourChangeStarted;
            ColourManager.OnColourChangeInstant -= HandleColourChangeInstant;
        }
    }
}
