using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Colour;
using Gameplay.Core;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Gameplay.Environment
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Platform : MonoBehaviour
    {
        [SerializeField] 
        private ColourId colourId;

        [SerializeField] 
        private float backgroundAlpha;

        [SerializeField] 
        private BoxCollider2D mainCollider;

        [SerializeField] 
        private SpriteRenderer mainSpriteRenderer;
        
        [SerializeField] 
        private SpriteRenderer backgroundSpriteRenderer;

        [SerializeField] 
        private AnimationCurve fadeCurve;
        
        [SerializeField] 
        private Vector2 backgroundOffset;
        
        [SerializeField] 
        private Vector2 hologramTilingRange;

        [SerializeField] 
        private Vector2 hologramSpeedRange;
        
        [SerializeField] 
        private Vector2 hologramStrengthRange;

        [SerializeField] 
        private ColourDatabase colourDatabase;

        [SerializeField, ReadOnly]
        private ColourConfig colourConfig;
        
        private bool isActive;
        
        private static readonly int ScrollSpeed = Shader.PropertyToID("_ScrollSpeed");
        private static readonly int Tiling = Shader.PropertyToID("_Tiling");
        private static readonly int Strength = Shader.PropertyToID("_Strength");

        private void Awake()
        {
            ColourManager.OnColourChangeStarted += HandleColourChangeStarted;
            ColourManager.OnColourChangeInstant += HandleColourChangeInstant;

            InitialiseHologramSettings();
        }
        
        private void InitialiseHologramSettings()
        {
            var hologramMaterial = backgroundSpriteRenderer.material;

            hologramMaterial.SetVector(Tiling, new Vector2(1f, Random.Range(hologramTilingRange.x, hologramTilingRange.y) * mainCollider.size.y));
            hologramMaterial.SetFloat(ScrollSpeed, Random.Range(hologramSpeedRange.x, hologramSpeedRange.y));
            hologramMaterial.SetFloat(Strength, Random.Range(hologramStrengthRange.x, hologramStrengthRange.y));
        }

        private void HandleColourChangeStarted(ColourId colour, float duration)
        {
            var shouldActivate = colourId == colour;

            if (shouldActivate != isActive)
            {
                isActive = shouldActivate;
                TogglePlatformAsync(duration).Forget();
            }
        }
        
        private void HandleColourChangeInstant(ColourId colour)
        {
            isActive = colourId == colour;
            TogglePlatformAsync(0f).Forget();
        }

        private async UniTask TogglePlatformAsync(float duration)
        {
            mainCollider.enabled = isActive;

            var initialTime = TimeManager.Instance.UnpausedRealtimeSinceStartup;
            var startColour = mainSpriteRenderer.color;
            var targetColour = isActive ? colourConfig.PlatformColour : Color.clear;
            var timeElapsed = 0f;

            // run independent of timescale, since this happens during the slowdown
            while (timeElapsed < duration)
            {
                var lerp = fadeCurve.Evaluate(timeElapsed / duration);

                mainSpriteRenderer.color = (1f - lerp) * startColour + lerp * targetColour;
                
                await UniTask.Yield();
                
                timeElapsed = TimeManager.Instance.UnpausedRealtimeSinceStartup - initialTime;
            }

            mainSpriteRenderer.color = targetColour;
        }

        private void OnDestroy()
        {
            ColourManager.OnColourChangeStarted -= HandleColourChangeStarted;
            ColourManager.OnColourChangeInstant -= HandleColourChangeInstant;
        }
        
#if UNITY_EDITOR
        [Button("Setup Platform")]
        private void SetupPlatform()
        {
            if (!colourDatabase.TryGetColourConfig(colourId, out colourConfig))
            {
                GameLogger.LogError($"Cannot fill tiles since the colour config for {colourId} could not be found in the colour database!", colourDatabase);
                return;
            }

            var platformSize = mainCollider.size;
            var rendererPosition = mainCollider.bounds.center.xy();
            var backgroundColour = colourConfig.Background;
            
            backgroundColour.a = backgroundAlpha;
            
            mainSpriteRenderer.size = platformSize;
            mainSpriteRenderer.transform.position = rendererPosition;
            mainSpriteRenderer.color = colourConfig.PlatformColour;
            
            backgroundSpriteRenderer.size = platformSize;
            backgroundSpriteRenderer.transform.position = rendererPosition + backgroundOffset;
            backgroundSpriteRenderer.color = backgroundColour;
        }
#endif
    }
}
