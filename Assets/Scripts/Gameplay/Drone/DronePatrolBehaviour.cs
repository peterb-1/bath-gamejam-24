using UnityEngine;

namespace Gameplay.Drone
{
    public class DronePatrolBehaviour : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private Transform patrolPoint1;

        [SerializeField]
        private Transform patrolPoint2;

        [SerializeField] 
        private float cycleTime;
        
        [SerializeField]
        private float cycleOffset;

        [SerializeField] 
        private bool smoothEnds;
        
        [Header("References")]
        [SerializeField] 
        private DroneHitboxBehaviour droneHitboxBehaviour;

        private float currentCycleTime;
        private bool isAlive = true;

        private void Awake()
        {
            droneHitboxBehaviour.OnDroneKilled += HandleDroneKilled;
            
            currentCycleTime = cycleTime * cycleOffset;
        }

        private void HandleDroneKilled(DroneHitboxBehaviour _)
        {
            isAlive = false;
        }

        private void Update()
        {
            if (!isAlive) return;
            
            currentCycleTime += Time.deltaTime;
            currentCycleTime %= cycleTime;

            var cycleProgress = currentCycleTime / cycleTime;
            var lerp = 1f - 2f * Mathf.Abs(cycleProgress - 0.5f);

            if (smoothEnds)
            {
                lerp = Mathf.SmoothStep(0f, 1f, lerp);
            }

            transform.position = lerp * patrolPoint1.position + (1f - lerp) * patrolPoint2.position;
        }

        private void OnDestroy()
        {
            droneHitboxBehaviour.OnDroneKilled -= HandleDroneKilled;
        }
    }
}
