using System;
using NaughtyAttributes;
using UnityEngine;

namespace Gameplay.Drone
{
    public class PatrolMovementStrategy : IDroneMovementStrategy
    {
        [SerializeField]
        private PatrolType patrolType;

        [SerializeField] 
        private float cycleTime;
        
        [SerializeField]
        private float cycleOffset;

        [SerializeField, ShowIf(nameof(patrolType), PatrolType.Linear), AllowNesting]
        private Transform patrolPoint1;

        [SerializeField, ShowIf(nameof(patrolType), PatrolType.Linear), AllowNesting]
        private Transform patrolPoint2;

        [SerializeField, ShowIf(nameof(patrolType), PatrolType.Linear), AllowNesting]
        private bool smoothEnds;
        
        [SerializeField, ShowIf(nameof(patrolType), PatrolType.Circular), AllowNesting]
        private Transform centre;
        
        [SerializeField, ShowIf(nameof(patrolType), PatrolType.Circular), AllowNesting]
        private float radius;
        
        [SerializeField, ShowIf(nameof(patrolType), PatrolType.Circular), AllowNesting]
        private bool isClockwise;
        
        private float currentCycleTime;
        private bool hasAppliedCycleOffset;
        
        public Vector3 GetUpdatedPosition()
        {
            if (!hasAppliedCycleOffset)
            {
                currentCycleTime = cycleOffset * cycleTime;
                hasAppliedCycleOffset = true;
            }
            
            currentCycleTime += Time.deltaTime;
            currentCycleTime %= cycleTime;

            var cycleProgress = currentCycleTime / cycleTime;

            return patrolType switch
            {
                PatrolType.Linear => GetLinearPosition(cycleProgress),
                PatrolType.Circular => GetCircularPosition(cycleProgress),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        private Vector3 GetLinearPosition(float cycleProgress)
        {
            var lerp = 1f - 2f * Mathf.Abs(cycleProgress - 0.5f);

            if (smoothEnds)
            {
                lerp = Mathf.SmoothStep(0f, 1f, lerp);
            }

            return lerp * patrolPoint1.position + (1f - lerp) * patrolPoint2.position;
        }
        
        private Vector3 GetCircularPosition(float cycleProgress)
        {
            var x = radius * Mathf.Cos(2f * Mathf.PI * cycleProgress);
            var y = radius * Mathf.Sin(2f * Mathf.PI * cycleProgress) * (isClockwise ? -1f : 1f);
            
            return centre.position + new Vector3(x, y);
        }
        
#if UNITY_EDITOR
        public void DrawGizmos()
        {
            if (patrolType is PatrolType.Linear)
            {
                Gizmos.color = Color.yellow;
                
                var lerp = 1f - 2f * Mathf.Abs(cycleOffset - 0.5f);

                if (smoothEnds)
                {
                    lerp = Mathf.SmoothStep(0f, 1f, lerp);
                }
            
                Gizmos.DrawSphere(lerp * patrolPoint1.position + (1f - lerp) * patrolPoint2.position, 0.25f);
                Gizmos.DrawLine(patrolPoint1.position, patrolPoint2.position);
            }

            if (patrolType is PatrolType.Circular)
            {
                Gizmos.color = Color.yellow;
            
                var x = radius * Mathf.Cos(2f * Mathf.PI * cycleOffset);
                var y = radius * Mathf.Sin(2f * Mathf.PI * cycleOffset) * (isClockwise ? -1f : 1f);

                Gizmos.DrawSphere(centre.position + new Vector3(x, y), 0.25f);
                Gizmos.DrawWireSphere(centre.position, radius);
            }
        }
#endif
    }
}