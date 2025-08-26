using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;

namespace Gameplay.Drone
{
    public class DronePatrolBehaviour : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private DronePatrolStrategy dronePatrolStrategy;

        [SerializeField] 
        private float cycleTime;
        
        [SerializeField]
        private float cycleOffset;

        [SerializeField, ShowIf(nameof(dronePatrolStrategy), DronePatrolStrategy.Linear)]
        private Transform patrolPoint1;

        [SerializeField, ShowIf(nameof(dronePatrolStrategy), DronePatrolStrategy.Linear)]
        private Transform patrolPoint2;

        [SerializeField, ShowIf(nameof(dronePatrolStrategy), DronePatrolStrategy.Linear)]
        private bool smoothEnds;
        
        [SerializeField, ShowIf(nameof(dronePatrolStrategy), DronePatrolStrategy.Circular)]
        private Transform centre;
        
        [SerializeField, ShowIf(nameof(dronePatrolStrategy), DronePatrolStrategy.Circular)]
        private float radius;
        
        [SerializeField, ShowIf(nameof(dronePatrolStrategy), DronePatrolStrategy.Circular)]
        private bool isClockwise;
        
        [Header("References")]
        [SerializeField] 
        private DroneHitboxBehaviour droneHitboxBehaviour;

        private float currentCycleTime;
        private bool isActive;
        private bool isAlive = true;

        private void Awake()
        {
            droneHitboxBehaviour.OnDroneKilled += HandleDroneKilled;
            droneHitboxBehaviour.OnDroneKilledByGhost += HandleDroneKilled;
            
            currentCycleTime = cycleTime * cycleOffset;

            isActive = droneHitboxBehaviour.StartActive;
        }

        private void HandleDroneKilled(DroneHitboxBehaviour _)
        {
            isAlive = false;
        }

        private void Update()
        {
            if (!isAlive || !isActive) return;
            
            currentCycleTime += Time.deltaTime;
            currentCycleTime %= cycleTime;

            var cycleProgress = currentCycleTime / cycleTime;

            switch (dronePatrolStrategy)
            {
                case DronePatrolStrategy.Linear:
                    SetLinearPosition(cycleProgress);
                    break;
                case DronePatrolStrategy.Circular:
                    SetCircularPosition(cycleProgress);
                    break;
            }
        }

        private void SetLinearPosition(float cycleProgress)
        {
            var lerp = 1f - 2f * Mathf.Abs(cycleProgress - 0.5f);

            if (smoothEnds)
            {
                lerp = Mathf.SmoothStep(0f, 1f, lerp);
            }

            transform.position = lerp * patrolPoint1.position + (1f - lerp) * patrolPoint2.position;
        }
        
        private void SetCircularPosition(float cycleProgress)
        {
            var x = radius * Mathf.Cos(2f * Mathf.PI * cycleProgress);
            var y = radius * Mathf.Sin(2f * Mathf.PI * cycleProgress) * (isClockwise ? -1f : 1f);
            
            transform.position = centre.position + new Vector3(x, y);
        }

        public void SetIsPatrolling(bool isPatrolling)
        {
            isActive = isPatrolling;
        }

        public async UniTask ActivateAsync()
        {
            isActive = true;
            
            await droneHitboxBehaviour.RunFadeInAsync();
        }
        
        private void OnDestroy()
        {
            droneHitboxBehaviour.OnDroneKilled -= HandleDroneKilled;
            droneHitboxBehaviour.OnDroneKilledByGhost -= HandleDroneKilled;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (dronePatrolStrategy is DronePatrolStrategy.Linear)
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

            if (dronePatrolStrategy is DronePatrolStrategy.Circular)
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
