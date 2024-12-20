using System;
using Cysharp.Threading.Tasks;
using Gameplay.Colour;
using Gameplay.Core;
using NaughtyAttributes;
using UnityEngine;

namespace Gameplay.Zipline
{
    public class ZiplineColourBehaviour : MonoBehaviour
    {
        [SerializeField]
        private bool isColoured;

        [SerializeField, ShowIf(nameof(isColoured))] 
        private ColourId colourId;
        
        [SerializeField] 
        private ZiplinePhysicsBehaviour ziplinePhysicsBehaviour;
        
        [SerializeField] 
        private LineRenderer lineRenderer;
        
        [SerializeField] 
        private ColourDatabase colourDatabase;
        
        [SerializeField] 
        private AnimationCurve fadeCurve;

        [SerializeField] 
        private float gradientSpeedMultiplier;

        [SerializeField] 
        private float fadedAlpha;

        private float gradientSpeed;
        private bool isActive = true;

        private void Awake()
        {
            gradientSpeed = Mathf.Sqrt(ziplinePhysicsBehaviour.TraversalSpeed) * gradientSpeedMultiplier;
            
            if (isColoured)
            {
                ColourManager.OnColourChangeStarted += HandleColourChangeStarted;
                ColourManager.OnColourChangeInstant += HandleColourChangeInstant;
                
                if (colourDatabase.TryGetColourConfig(colourId, out var colourConfig))
                {
                    lineRenderer.colorGradient = colourConfig.ZiplineGradient;
                }
            }
            else
            {
                ziplinePhysicsBehaviour.SetActive(true);
            }
        }

        private void HandleColourChangeStarted(ColourId colour, float duration)
        {
            var shouldActivate = colourId == colour;

            if (shouldActivate != isActive)
            {
                isActive = shouldActivate;
                ToggleZiplineAsync(duration).Forget();
            }
        }
        
        private void HandleColourChangeInstant(ColourId colour)
        {
            isActive = colourId == colour;
            ToggleZiplineAsync(0f).Forget();
        }

        private void Update()
        {
            UpdateGradient();
        }

        private void UpdateGradient()
        {
            var colourKeys = lineRenderer.colorGradient.colorKeys;
            var newColourKeys = new GradientColorKey[colourKeys.Length];
            
            for (var i = 0; i < colourKeys.Length; i++)
            {
                newColourKeys[i] = colourKeys[i];
            }

            for (var i = 1; i < newColourKeys.Length - 1; i++)
            {
                var colourKey = newColourKeys[i];
                
                colourKey.time += gradientSpeed * Time.deltaTime;
                colourKey.time %= 1f;
                    
                newColourKeys[i] = colourKey;
            }
            
            Array.Sort(newColourKeys, (a, b) => a.time.CompareTo(b.time));

            var secondKey = newColourKeys[1];
            var penultimateKey = newColourKeys[^2];
            var secondKeyContribution = 1f - penultimateKey.time;
            var penultimateKeyContribution = secondKey.time;

            if (secondKeyContribution + penultimateKeyContribution > 0f)
            {
                var unnormalisedColour = secondKey.color * secondKeyContribution + penultimateKey.color * penultimateKeyContribution;
                var endColour = unnormalisedColour / (secondKeyContribution + penultimateKeyContribution);

                newColourKeys[0].color = endColour;
                newColourKeys[^1].color = endColour;
            }

            var newGradient = lineRenderer.colorGradient;
            newGradient.colorKeys = newColourKeys;
            lineRenderer.colorGradient = newGradient;
        }
        
        private async UniTask ToggleZiplineAsync(float duration)
        {
            ziplinePhysicsBehaviour.SetActive(isActive);
            
            var initialTime = TimeManager.Instance.UnpausedRealtimeSinceStartup;
            var startAlpha = lineRenderer.colorGradient.colorKeys[0].color.a;
            var targetAlpha = isActive ? 1f : fadedAlpha;
            var timeElapsed = 0f;

            // run fade independent of timescale, since this happens during the slowdown
            while (timeElapsed < duration)
            {
                var lerp = fadeCurve.Evaluate(timeElapsed / duration);

                SetLineAlpha((1f - lerp) * startAlpha + lerp * targetAlpha);
                
                await UniTask.Yield();
                
                timeElapsed = TimeManager.Instance.UnpausedRealtimeSinceStartup - initialTime;
            }

            SetLineAlpha(targetAlpha);
        }

        private void SetLineAlpha(float alpha)
        {
            var originalGradient = lineRenderer.colorGradient;
            var colourKeys = originalGradient.colorKeys;
            var alphaKeys = originalGradient.alphaKeys;

            for (var i = 0; i < alphaKeys.Length; i++)
            {
                alphaKeys[i].alpha = alpha;
            }

            var newGradient = new Gradient
            {
                colorKeys = colourKeys,
                alphaKeys = alphaKeys
            };

            lineRenderer.colorGradient = newGradient;
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