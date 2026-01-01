using UnityEngine;
using Utils;

namespace Gameplay.Drone
{
    public class DroneMovementBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private DroneHitboxBehaviour droneHitboxBehaviour;
        
        [field: SerializeField] 
        public bool ShouldStartActive { get; private set; }
        
        [SerializeReference, SubclassSelector] 
        private IDroneMovementStrategy movementStrategy;

        public IDroneMovementStrategy MovementStrategy => movementStrategy;

        private bool isActive;
        private bool isAlive = true;

        private void Awake()
        {
            droneHitboxBehaviour.OnDroneKilled += HandleDroneKilled;
            droneHitboxBehaviour.OnDroneKilledByGhost += HandleDroneKilled;

            isActive = ShouldStartActive;

            droneHitboxBehaviour.SetActive(isActive);
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

        public void Activate(bool shouldAnimate = false)
        {
            isActive = true;
            droneHitboxBehaviour.SetActive(true, shouldAnimate);
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
