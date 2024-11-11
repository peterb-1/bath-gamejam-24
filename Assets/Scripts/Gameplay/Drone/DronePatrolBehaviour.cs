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
        private bool smoothEnds;
        
        [Header("References")]
        [SerializeField] 
        private DroneHitboxBehaviour droneHitboxBehaviour;

        private float currentCycleTime = 0f;
        private bool isAlive = true;

        private void Awake()
        {
            droneHitboxBehaviour.OnDroneKilled += HandleDroneKilled;
        }

        private void HandleDroneKilled()
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
