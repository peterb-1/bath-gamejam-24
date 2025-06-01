using Cysharp.Threading.Tasks;
using Gameplay.Colour;
using Gameplay.Core;
using NaughtyAttributes;
using UnityEngine;
using Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gameplay.Environment
{
    public class LaserBlock : MonoBehaviour
    {
        [SerializeField]
        private bool isColoured;

        [SerializeField, ShowIf(nameof(isColoured))] 
        private ColourId colourId;
        
        [SerializeField] 
        private SpriteRenderer backgroundRenderer;
        
        [SerializeField] 
        private SpriteRenderer powerIconsRenderer;

        [SerializeField] 
        private Collider2D deathCollider;

        [SerializeField] 
        private Laser laser1;
        
        [SerializeField] 
        private Laser laser2;
        
        [SerializeField] 
        private Laser laser3;
        
        [SerializeField] 
        private Laser laser4;
        
        [SerializeField] 
        private AnimationCurve fadeCurve;
        
        [SerializeField, Range(0f, 1f)] 
        private float backgroundAlpha;
        
        [SerializeField, Range(0f, 1f)] 
        private float fadedAlpha;
        
        [SerializeField] 
        private ColourDatabase colourDatabase;

        private bool isActive;
        
        private void Awake()
        {
            if (isColoured)
            {
                ColourManager.OnColourChangeStarted += HandleColourChangeStarted;
                ColourManager.OnColourChangeInstant += HandleColourChangeInstant;
            }
        }

        private void HandleColourChangeStarted(ColourId colour, float duration)
        {
            var shouldActivate = colourId == colour;

            if (shouldActivate != isActive)
            {
                isActive = shouldActivate;
                ToggleBlockAsync(duration).Forget();
            }
        }

        private void HandleColourChangeInstant(ColourId colour)
        {
            isActive = colourId == colour;
            ToggleBlockAsync(0f).Forget();
        }

        private async UniTask ToggleBlockAsync(float duration)
        {
            deathCollider.enabled = isActive;
            
            var initialTime = TimeManager.Instance.UnpausedRealtimeSinceStartup;
            var currentColour = powerIconsRenderer.color;
            var startAlpha = currentColour.a;
            var targetAlpha = isActive ? 1f : fadedAlpha;
            var timeElapsed = 0f;

            // run fade independent of timescale, since this happens during the slowdown
            while (timeElapsed < duration)
            {
                var lerp = fadeCurve.Evaluate(timeElapsed / duration);

                currentColour.a = (1f - lerp) * startAlpha + lerp * targetAlpha;
                
                powerIconsRenderer.color = currentColour;
                
                await UniTask.Yield();
                
                timeElapsed = TimeManager.Instance.UnpausedRealtimeSinceStartup - initialTime;
            }

            currentColour.a = targetAlpha;
            
            powerIconsRenderer.color = currentColour;
        }

        private void OnDestroy()
        {
            if (isColoured)
            {
                ColourManager.OnColourChangeStarted -= HandleColourChangeStarted;
                ColourManager.OnColourChangeInstant -= HandleColourChangeInstant;
            }
        }
        
#if UNITY_EDITOR
        [Button("Setup Laser Block")]
        private void SetupLaserBlock()
        {
            var scale = backgroundRenderer.size;
            var offset = scale * 0.5f;
            var centre = backgroundRenderer.transform.position.xy();

            var topLeft = new Vector2(centre.x - offset.x, centre.y + offset.y);
            var topRight = new Vector2(centre.x + offset.x, centre.y + offset.y);
            var bottomRight = new Vector2(centre.x + offset.x, centre.y - offset.y);
            var bottomLeft = new Vector2(centre.x - offset.x, centre.y - offset.y);

            laser1.SetupForLaserBlock(topLeft, topRight, isColoured, colourId);
            laser2.SetupForLaserBlock(topRight, bottomRight, isColoured, colourId);
            laser3.SetupForLaserBlock(bottomRight, bottomLeft, isColoured, colourId);
            laser4.SetupForLaserBlock(bottomLeft, topLeft, isColoured, colourId);

            powerIconsRenderer.size = scale;
            deathCollider.transform.localScale = scale.WithZ(1f);

            if (isColoured && colourDatabase.TryGetColourConfig(colourId, out var colourConfig))
            {
                var backgroundColour = colourConfig.BackgroundDesaturated;
                backgroundColour.a = backgroundAlpha;

                backgroundRenderer.color = backgroundColour;
                powerIconsRenderer.color = colourConfig.LaserColour;
            }
            
            EditorUtility.SetDirty(laser1);
            EditorUtility.SetDirty(laser2);
            EditorUtility.SetDirty(laser3);
            EditorUtility.SetDirty(laser4);
            EditorUtility.SetDirty(this);
        }
#endif
    }
}