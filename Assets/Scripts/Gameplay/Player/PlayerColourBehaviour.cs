using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Colour;
using Gameplay.Core;
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
        private TrailRenderer trailRenderer;
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
        
        private void HandleTrailLoaded(TrailRenderer trail)
        {
            trailRenderer = trail;
            trailGradient = trailRenderer.colorGradient;

            if (currentColourConfig != null)
            {
                trailRenderer.colorGradient = trailGradient.WithTint(currentColourConfig.PlayerColour);
            }
        }

        private void SetColour(ColourConfig colourConfig)
        {
            currentColourConfig = colourConfig;
            
            playerSpriteRenderer.color = colourConfig.PlayerColour;
            deathParticleRenderer.material.color = colourConfig.PlayerColour;

            if (trailRenderer != null)
            {
                trailRenderer.colorGradient = trailGradient.WithTint(colourConfig.PlayerColour);
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
                trailRenderer.colorGradient = trailGradient.WithTint(colour);
                
                await UniTask.Yield();
                
                timeElapsed = TimeManager.Instance.UnpausedRealtimeSinceStartup - initialTime;
            }

            playerSpriteRenderer.color = startColour;
            trailRenderer.colorGradient = trailGradient.WithTint(startColour);
        }

        private void OnDestroy()
        {
            ColourManager.OnColourChangeStarted -= HandleColourChangeStarted;
            ColourManager.OnColourChangeInstant -= HandleColourChangeInstant;
            
            playerTrailBehaviour.OnTrailLoaded -= HandleTrailLoaded;
        }
    }
}
