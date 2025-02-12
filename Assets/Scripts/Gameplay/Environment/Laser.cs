using Cysharp.Threading.Tasks;
using Gameplay.Colour;
using Gameplay.Core;
using NaughtyAttributes;
using UnityEngine;

namespace Gameplay.Environment
{
    public class Laser : MonoBehaviour
    {
        [SerializeField]
        private bool isColoured;
        
        [SerializeField, ShowIf(nameof(isColoured))] 
        private bool isStartColoured;
        
        [SerializeField, ShowIf(nameof(isColoured))] 
        private bool isEndColoured;

        [SerializeField, ShowIf(nameof(isColoured))] 
        private ColourId colourId;

        [SerializeField] 
        private AnimationCurve fadeCurve;

        [SerializeField] 
        private float fadedAlpha;
    
        [SerializeField] 
        private Collider2D deathCollider;

        [SerializeField] 
        private SpriteRenderer laserRenderer;

        [SerializeField] 
        private ParticleSystem distortionParticles;
        
        [SerializeField]
        private SpriteRenderer[] endpointCoreRenderers;
        
        [SerializeField] 
        private SpriteRenderer[] endpointRenderers;
        
        [SerializeField] 
        private ParticleSystem[] endpointParticleSystems;

        [SerializeField] 
        private float endpointOffset;
        
        [SerializeField] 
        private float tilingInterval;

        [SerializeField] 
        private float particlesPerUnitLength;

        [SerializeField] 
        private ColourDatabase colourDatabase;

        private bool isActive = true;
        
        private static readonly int Tiling = Shader.PropertyToID("_Tiling");

        private void Awake()
        {
            if (isColoured)
            {
                ColourManager.OnColourChangeStarted += HandleColourChangeStarted;
                ColourManager.OnColourChangeInstant += HandleColourChangeInstant;
                
                if (colourDatabase.TryGetColourConfig(colourId, out var colourConfig))
                {
                    laserRenderer.color = colourConfig.LaserColour;
                    
                    foreach (var spriteRenderer in endpointRenderers)
                    {
                        spriteRenderer.color = colourConfig.LaserColour;
                    }

                    if (isStartColoured) endpointCoreRenderers[0].color = colourConfig.LaserColour;
                    if (isEndColoured) endpointCoreRenderers[1].color = colourConfig.LaserColour;

                    foreach (var particles in endpointParticleSystems)
                    {
                        var main = particles.main;
                        main.startColor = colourConfig.LaserColour;
                    }
                }
                
                foreach (var coreRenderer in endpointCoreRenderers)
                {
                    // might be rotated inside prefab, so ensure that the lightning icon is the right way up
                    coreRenderer.transform.rotation = Quaternion.identity;
                }
                
                var length = (endpointRenderers[0].transform.position - endpointRenderers[1].transform.position).magnitude;
                laserRenderer.material.SetFloat(Tiling, length / tilingInterval);
            }
        }

        private void HandleColourChangeStarted(ColourId colour, float duration)
        {
            var shouldActivate = colourId == colour;

            if (shouldActivate != isActive)
            {
                isActive = shouldActivate;
                ToggleLaserAsync(duration).Forget();
            }
        }

        private void HandleColourChangeInstant(ColourId colour)
        {
            isActive = colourId == colour;
            ToggleLaserAsync(0f).Forget();
        }

        private async UniTask ToggleLaserAsync(float duration)
        {
            deathCollider.enabled = isActive;
            
            var initialTime = TimeManager.Instance.UnpausedRealtimeSinceStartup;
            var currentColour = laserRenderer.color;
            var startAlpha = currentColour.a;
            var targetAlpha = isActive ? 1f : fadedAlpha;
            var timeElapsed = 0f;

            // run fade independent of timescale, since this happens during the slowdown
            while (timeElapsed < duration)
            {
                var lerp = fadeCurve.Evaluate(timeElapsed / duration);

                currentColour.a = (1f - lerp) * startAlpha + lerp * targetAlpha;
                
                SetColour(currentColour);
                
                await UniTask.Yield();
                
                timeElapsed = TimeManager.Instance.UnpausedRealtimeSinceStartup - initialTime;
            }

            currentColour.a = targetAlpha;
            
            SetColour(currentColour);
        }

        private void SetColour(Color colour)
        {
            laserRenderer.color = colour;
            
            foreach (var particles in endpointParticleSystems)
            {
                var main = particles.main;
                main.startColor = colour;
            }
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
        [Button("Setup Laser")]
        private void SetupLaser()
        {
            var mainTransform = laserRenderer.transform;
            var distortionTransform = distortionParticles.transform;
            var startPos = endpointRenderers[0].transform.position;
            var endPos = endpointRenderers[1].transform.position;
            var midPos = (startPos + endPos) / 2;
            var direction = endPos - startPos;
            var angle = Mathf.Rad2Deg * Mathf.Atan2(direction.y, direction.x);
            var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            var currentScale = mainTransform.localScale;
            var rectScale = new Vector3(direction.magnitude - 2f * endpointOffset, currentScale.y, 1f);

            mainTransform.position = midPos;
            mainTransform.rotation = rotation;
            mainTransform.localScale = rectScale;
            
            distortionTransform.position = midPos;
            distortionTransform.rotation = rotation;
            distortionTransform.localScale = rectScale;

            endpointRenderers[0].transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
            endpointRenderers[1].transform.rotation = Quaternion.AngleAxis(angle + 90f, Vector3.forward);

            var emission = distortionParticles.emission;
            emission.rateOverTime = particlesPerUnitLength * direction.magnitude;
        }
#endif
    }
}