using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils;

namespace Gameplay.Drone
{
    public class DroneMovementBehaviour : MonoBehaviour
    {
        [Header("Settings")] 
        [SerializeReference, SubclassSelector] 
        private IDroneMovementStrategy movementStrategy;

        [SerializeField] 
        private float cycleTime;
        
        [SerializeField]
        private float cycleOffset;

        [SerializeField]
        private bool smoothEnds;
        
        [SerializeField]
        private float radius;
        
        [SerializeField]
        private bool isClockwise;
        
        [SerializeField]
        private Transform patrolPoint1;

        [SerializeField]
        private Transform patrolPoint2;
        
        [Header("References")]
        [SerializeField] 
        private DroneHitboxBehaviour droneHitboxBehaviour;
        
        public IDroneMovementStrategy MovementStrategy => movementStrategy;

        private bool isActive;
        private bool isAlive = true;

        private void Awake()
        {
            droneHitboxBehaviour.OnDroneKilled += HandleDroneKilled;
            droneHitboxBehaviour.OnDroneKilledByGhost += HandleDroneKilled;

            isActive = droneHitboxBehaviour.StartActive;
        }

        private void HandleDroneKilled(DroneHitboxBehaviour _)
        {
            isAlive = false;
        }

        private void Update()
        {
            if (!isAlive || !isActive) return;
            
            transform.position = movementStrategy.GetUpdatedPosition();
        }

        public void SetMovementStrategy(IDroneMovementStrategy newStrategy)
        {
            movementStrategy = newStrategy;
        }

        public void Activate()
        {
            isActive = true;
            droneHitboxBehaviour.RunFadeInAsync().Forget();
        }
        
        private void OnDestroy()
        {
            droneHitboxBehaviour.OnDroneKilled -= HandleDroneKilled;
            droneHitboxBehaviour.OnDroneKilledByGhost -= HandleDroneKilled;
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            movementStrategy?.DrawGizmos();
        }
#endif
    }
}
