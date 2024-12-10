using Cysharp.Threading.Tasks;
using Gameplay.Colour;
using Gameplay.Core;
using NaughtyAttributes;
using UnityEngine;

namespace Gameplay.Drone
{
    public class DroneColourBehaviour : MonoBehaviour
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
        private Collider2D deathCollider;

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
                    spriteRenderer.color = colourConfig.DroneColour;
                }
            }
        }

        private void HandleColourChangeStarted(ColourId colour, float duration)
        {
            var shouldActivate = colourId == colour;

            if (shouldActivate != isActive)
            {
                isActive = shouldActivate;
                ToggleDroneAsync(duration).Forget();
            }
        }

        private void HandleColourChangeInstant(ColourId colour)
        {
            isActive = colourId == colour;
            ToggleDroneAsync(0f).Forget();
        }

        private async UniTask ToggleDroneAsync(float duration)
        {
            deathCollider.enabled = isActive;
            
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


        /*
    #if UNITY_EDITOR
    [Button("Colour Drone")]
    private void ColourDrone()
    {
        if(isColoured)
        {
            // write code
        }
    }
    #endif
    */
    }
}
