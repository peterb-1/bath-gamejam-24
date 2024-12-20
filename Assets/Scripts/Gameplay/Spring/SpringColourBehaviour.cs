using Cysharp.Threading.Tasks;
using Gameplay.Colour;
using Gameplay.Core;
using NaughtyAttributes;
using UnityEngine;

namespace Gameplay.Spring
{
    public class SpringColourBehaviour : MonoBehaviour
    {
        [SerializeField]
        private bool isColoured;

        [SerializeField, ShowIf(nameof(isColoured))] 
        private ColourId colourId;

        [SerializeField] 
        private AnimationCurve fadeCurve;

        [SerializeField] 
        private float fadedAlpha;
    
        [SerializeField] 
        private Collider2D springCollider;

        [SerializeField] 
        private SpriteRenderer spriteRenderer;

        [SerializeField] 
        private ColourDatabase colourDatabase;

        private bool isActive = true;

        private void Awake()
        {
            if (isColoured)
            {
                ColourManager.OnColourChangeStarted += HandleColourChangeStarted;
                ColourManager.OnColourChangeInstant += HandleColourChangeInstant;
                
                if (colourDatabase.TryGetColourConfig(colourId, out var colourConfig))
                {
                    spriteRenderer.color = colourConfig.SpringColour;
                }
            }
        }

        private void HandleColourChangeStarted(ColourId colour, float duration)
        {
            var shouldActivate = colourId == colour;

            if (shouldActivate != isActive)
            {
                isActive = shouldActivate;
                ToggleSpringAsync(duration).Forget();
            }
        }

        private void HandleColourChangeInstant(ColourId colour)
        {
            isActive = colourId == colour;
            ToggleSpringAsync(0f).Forget();
        }

        private async UniTask ToggleSpringAsync(float duration)
        {
            springCollider.enabled = isActive;
            
            var initialTime = TimeManager.Instance.UnpausedRealtimeSinceStartup;
            var startColour = spriteRenderer.color;
            var startAlpha = startColour.a;
            var targetAlpha = isActive ? 1f : fadedAlpha;
            var timeElapsed = 0f;

            // run fade independent of timescale, since this happens during the slowdown
            while (timeElapsed < duration)
            {
                var lerp = fadeCurve.Evaluate(timeElapsed / duration);

                startColour.a = (1f - lerp) * startAlpha + lerp * targetAlpha;
                spriteRenderer.color = startColour;
                
                await UniTask.Yield();
                
                timeElapsed = TimeManager.Instance.UnpausedRealtimeSinceStartup - initialTime;
            }

            startColour.a = targetAlpha;
            spriteRenderer.color = startColour;
        }

        private void OnDestroy()
        {
            if (isColoured)
            {
                ColourManager.OnColourChangeStarted -= HandleColourChangeStarted;
                ColourManager.OnColourChangeInstant -= HandleColourChangeInstant;
            }
        }
    }
}
