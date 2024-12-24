using Cysharp.Threading.Tasks;
using Gameplay.Colour;
using Gameplay.Core;
using UnityEngine;

namespace Gameplay.Environment
{
    public class ColouredSceneryItem : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField] 
        private ColourId colourId;
        
        [SerializeField] 
        private AnimationCurve fadeCurve;
        
        [SerializeField] 
        private float fadedAlpha;

        private bool isActive;

        private void Awake()
        {
            ColourManager.OnColourChangeStarted += HandleColourChangeStarted;
            ColourManager.OnColourChangeInstant += HandleColourChangeInstant;
        }

        private void HandleColourChangeStarted(ColourId colour, float duration)
        {
            var shouldActivate = colourId == colour;

            if (shouldActivate != isActive)
            {
                isActive = shouldActivate;
                ToggleSceneryAsync(duration).Forget();
            }
        }

        private void HandleColourChangeInstant(ColourId colour)
        {
            isActive = colourId == colour;
            ToggleSceneryAsync(0f).Forget();
        }

        private async UniTask ToggleSceneryAsync(float duration)
        {
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
            ColourManager.OnColourChangeStarted -= HandleColourChangeStarted;
            ColourManager.OnColourChangeInstant -= HandleColourChangeInstant;
        }
    }
}