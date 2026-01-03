using UnityEngine;
using Utils;

namespace Gameplay.Drone
{
    public class DroneMovementBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private DroneHitboxBehaviour droneHitboxBehaviour;

        [SerializeField] 
        private float strategySwitchDuration;
        
        [field: SerializeField] 
        public bool ShouldStartActive { get; private set; }
        
        [SerializeReference, SubclassSelector] 
        private IDroneMovementStrategy movementStrategy;

        public IDroneMovementStrategy MovementStrategy => movementStrategy;

        private Vector3 transitionStartPosition;
        private Vector3 transitionStartVelocity;

        private float strategySwitchCountdown;
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
            
            movementStrategy.Update();
            
            var targetPosition = movementStrategy.GetPosition();

            if (strategySwitchCountdown > 0f)
            {
                var lerp = 1f - strategySwitchCountdown / strategySwitchDuration;
        
                if (movementStrategy is IFixedPathStrategy fixedPathStrategy)
                {
                    var timeRemaining = strategySwitchCountdown;
                        
                    var endPosition = fixedPathStrategy.GetPositionAfterTime(timeRemaining);
                    var endVelocity = fixedPathStrategy.GetVelocityAfterTime(timeRemaining);
                    
                    targetPosition = HermiteCurveUtils.CubicHermite(
                        transitionStartPosition, 
                        transitionStartVelocity * strategySwitchDuration,
                        endPosition, 
                        endVelocity * strategySwitchDuration,
                        lerp
                    );
                }
                else
                {
                    var desiredVelocity = movementStrategy.GetVelocity();
                    var blendedVelocity = Vector3.Lerp(transitionStartVelocity, desiredVelocity, lerp);
            
                    targetPosition = transform.position + blendedVelocity * Time.deltaTime;
                }
        
                strategySwitchCountdown -= Time.deltaTime;
            }

            transform.position = targetPosition;
        }

        public void SetMovementStrategy(IDroneMovementStrategy newStrategy)
        {
            transitionStartPosition = transform.position;
            transitionStartVelocity = movementStrategy.GetVelocity();
            
            movementStrategy = newStrategy;
            strategySwitchCountdown = strategySwitchDuration;
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
