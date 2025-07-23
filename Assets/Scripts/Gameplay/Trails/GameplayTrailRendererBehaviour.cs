using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Core;
using Gameplay.Player;
using UnityEngine;
using Utils;

namespace Gameplay.Trails
{
    public class GameplayTrailRendererBehaviour : AbstractGameplayTrailBehaviour
    {
        [field: SerializeField] 
        public TrailRenderer TrailRenderer { get; private set; }
        
        [Header("Emission")]
        [SerializeField] 
        private float emissionVelocityThreshold;

        [SerializeField] 
        private float maxTrailWidthVelocity;

        [SerializeField] 
        private float maxEmissionBoost;
        
        [SerializeField] 
        private float trailWidthResponseTime;
        
        [SerializeField, Range(0f, 1f)] 
        private float smoothingStrength;
        
        [Header("Wind")]
        [SerializeField] 
        private float windBaseStrength;
        
        [SerializeField] 
        private Vector2 windFrequency;
        
        [SerializeField] 
        private Vector2 windNoiseScale;

        [SerializeField] 
        private Vector2 windDirection;
        
        [SerializeField] 
        private AnimationCurve windFalloff;
        
        [Header("Environment")]
        [SerializeField]
        private LayerMask obstacleLayerMask;
        
        [SerializeField]
        private float environmentInteractionDistance;
        
        [SerializeField]
        private float environmentDeflectionStrength;
        
        private PlayerMovementBehaviour playerMovementBehaviour;
        
        private Vector2 lastPosition;
        private Vector2 trailVelocity;
        private ContactFilter2D contactFilter;
        private Gradient trailGradient;
        
        private readonly Vector3[] trailPoints = new Vector3[64];
        private readonly Collider2D[] colliderResults = new Collider2D[10];
        private readonly List<Vector2> environmentInfluencePoints = new();
        
        private int trailPointCount;
        private float currentVelocityFactor;
        private float time;
        private float originalWidthMultiplier = 1f;
        private bool isReady;

        private async void Awake()
        {
            contactFilter.SetLayerMask(obstacleLayerMask);
            contactFilter.useTriggers = false;
            
            originalWidthMultiplier = TrailRenderer.widthMultiplier;
            lastPosition = transform.position;
            trailGradient = TrailRenderer.colorGradient;

            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerMovementBehaviour = PlayerAccessService.Instance.PlayerMovementBehaviour;
            isReady = true;
        }
        
        private void Update()
        {
            if (!isReady || (PauseManager.Instance != null && PauseManager.Instance.IsPaused)) return;
            
            time += Time.deltaTime;
            
            var velocityMagnitude = playerMovementBehaviour.Velocity.magnitude;
            var targetVelocityFactor = Mathf.Clamp01(velocityMagnitude / maxTrailWidthVelocity);
            
            currentVelocityFactor = Mathf.Lerp(currentVelocityFactor, targetVelocityFactor, Time.deltaTime / trailWidthResponseTime);
            
            TrailRenderer.emitting = velocityMagnitude > emissionVelocityThreshold;
            
            if (TrailRenderer.widthCurve != null)
            {
                var widthMultiplier = Mathf.Lerp(originalWidthMultiplier, originalWidthMultiplier * maxEmissionBoost, currentVelocityFactor);
                TrailRenderer.widthMultiplier = widthMultiplier;
            }
            
            GatherEnvironmentInfluencePoints();
        }

        private void GatherEnvironmentInfluencePoints()
        {
            environmentInfluencePoints.Clear();
            
            var position = transform.position;
            
            var colliderCount = Physics2D.OverlapCircle(position, environmentInteractionDistance, contactFilter, colliderResults);
            
            for (var i = 0; i < colliderCount; i++)
            {
                var nearbyCollider = colliderResults[i];
                var closestPoint = nearbyCollider.ClosestPoint(position);
                var distance = Vector2.Distance(position, closestPoint);
                
                if (distance < environmentInteractionDistance)
                {
                    environmentInfluencePoints.Add(closestPoint);
                }
            }
        }

        private void LateUpdate()
        {
            if (PauseManager.Instance != null && PauseManager.Instance.IsPaused) return;
            
            trailPointCount = TrailRenderer.GetPositions(trailPoints);
            
            var currentPosition = (Vector2) transform.position;
            var currentVelocity = (currentPosition - lastPosition) / Time.deltaTime;
            
            trailVelocity = Vector2.Lerp(trailVelocity, currentVelocity, Time.deltaTime * 5f);
            lastPosition = currentPosition;
            
            SmoothTrailPoints();
            
            for (var i = 0; i < trailPointCount; i++)
            {
                var t = i / (float) trailPointCount;
                
                var windOffset = CalculateWindOffset(i, t);
                var environmentOffset = CalculateEnvironmentOffset(trailPoints[i]);
                
                trailPoints[i] += new Vector3(windOffset.x + environmentOffset.x, windOffset.y + environmentOffset.y, 0);
            }
            
            TrailRenderer.SetPositions(trailPoints);
        }
        
        private void SmoothTrailPoints()
        {
            if (trailPointCount < 3) return;

            for (var i = 1; i < trailPointCount - 1; i++)
            {
                var prev = trailPoints[i - 1];
                var current = trailPoints[i];
                var next = trailPoints[i + 1];

                trailPoints[i] = Vector3.Lerp(current, (prev + current + next) / 3f, smoothingStrength);
            }
        }

        private Vector2 CalculateWindOffset(int pointIndex, float normalizedDistance)
        {
            var noiseTimeX = time * windFrequency.x;
            var noiseTimeY = time * windFrequency.y;
            var noiseX = Mathf.PerlinNoise(pointIndex * windNoiseScale.x, noiseTimeX) - 0.5f;
            var noiseY = Mathf.PerlinNoise(pointIndex * windNoiseScale.y, noiseTimeY) - 0.5f;
            var strength = windBaseStrength * windFalloff.Evaluate(normalizedDistance);
            
            var windNoise = new Vector2(noiseX * windDirection.x, noiseY * windDirection.y) * strength;
            
            if (trailVelocity.magnitude > 0.1f)
            {
                var angle = Mathf.Atan2(trailVelocity.y, trailVelocity.x);
                var velocityFactor = Mathf.Min(1f, trailVelocity.magnitude * 0.1f);
                var rotatedWindX = windNoise.x * Mathf.Cos(angle) - windNoise.y * Mathf.Sin(angle);
                var rotatedWindY = windNoise.x * Mathf.Sin(angle) + windNoise.y * Mathf.Cos(angle);
                
                windNoise = new Vector2(rotatedWindX, rotatedWindY) * velocityFactor;
            }
            
            return windNoise;
        }

        private Vector2 CalculateEnvironmentOffset(Vector2 pointPosition)
        {
            if (environmentInfluencePoints.Count == 0) return Vector2.zero;
            
            var totalOffset = Vector2.zero;
            
            foreach (var influencePoint in environmentInfluencePoints)
            {
                var distance = Vector2.Distance(pointPosition, influencePoint);
                
                if (distance < environmentInteractionDistance)
                {
                    var direction = (pointPosition - influencePoint).normalized;
                    var deflectionFactor = 1f - distance / environmentInteractionDistance;
                    
                    totalOffset += direction * (deflectionFactor * environmentDeflectionStrength);
                }
            }
            
            return totalOffset;
        }

        public override void SetColour(Color colour)
        {
            TrailRenderer.colorGradient = trailGradient.WithTint(colour);
        }
    }
}