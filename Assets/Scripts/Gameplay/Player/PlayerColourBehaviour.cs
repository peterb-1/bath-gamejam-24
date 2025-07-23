using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Colour;
using Gameplay.Core;
using Gameplay.Trails;
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
        private PlayerTrailBehaviour playerTrailBehaviour;

        [SerializeField] 
        private ColourDatabase colourDatabase;

        [SerializeField] 
        private AnimationCurve flashCurve;

        private ColourConfig currentColourConfig;
        private AbstractGameplayTrailBehaviour trailBehaviour;
        private Gradient trailGradient;
        
        private void Awake()
        {
            ColourManager.OnColourChangeStarted += HandleColourChangeStarted;
            ColourManager.OnColourChangeInstant += HandleColourChangeInstant;

            playerTrailBehaviour.OnTrailLoaded += HandleTrailLoaded;
        }

        private void HandleColourChangeInstant(ColourId colour)
        {
            if (colourDatabase.TryGetColourConfig(colour, out var colourConfig, district: SceneLoader.Instance.CurrentDistrict))
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
            if (colourDatabase.TryGetColourConfig(colour, out var colourConfig, district: SceneLoader.Instance.CurrentDistrict))
            {
                SetColour(colourConfig);
                RunFlashAsync(duration).Forget();
            }
            else
            {
                GameLogger.LogError($"Cannot change player colour since the colour config for {colour} could not be found in the colour database!", colourDatabase);
            }
        }
        
        private void HandleTrailLoaded(AbstractGameplayTrailBehaviour trail)
        {
            trailBehaviour = trail;

            if (currentColourConfig != null)
            {
                trailBehaviour.SetColour(currentColourConfig.PlayerColour);
            }
        }

        private void SetColour(ColourConfig colourConfig)
        {
            currentColourConfig = colourConfig;
            
            playerSpriteRenderer.color = colourConfig.PlayerColour;
            deathParticleRenderer.material.color = colourConfig.PlayerColour;

            if (trailBehaviour != null)
            {
                trailBehaviour.SetColour(colourConfig.PlayerColour);
            }
        }

        private async UniTask RunFlashAsync(float duration)
        {
            var initialTime = TimeManager.Instance.UnpausedRealtimeSinceStartup;
            var startColour = playerSpriteRenderer.color;
            var timeElapsed = 0f;

            // run flash independent of timescale, since this happens during the slowdown
            while (timeElapsed < duration)
            {
                var lerp = flashCurve.Evaluate(timeElapsed / duration);
                var colour = (1f - lerp) * startColour + lerp * Color.white;

                playerSpriteRenderer.color = colour;

                if (trailBehaviour != null)
                {
                    trailBehaviour.SetColour(colour);
                }

                await UniTask.Yield();
                
                timeElapsed = TimeManager.Instance.UnpausedRealtimeSinceStartup - initialTime;
            }

            playerSpriteRenderer.color = startColour;

            if (trailBehaviour != null)
            {
                trailBehaviour.SetColour(startColour);
            }
        }

        private void OnDestroy()
        {
            ColourManager.OnColourChangeStarted -= HandleColourChangeStarted;
            ColourManager.OnColourChangeInstant -= HandleColourChangeInstant;
            
            playerTrailBehaviour.OnTrailLoaded -= HandleTrailLoaded;
        }
    }
}
