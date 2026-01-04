using Cysharp.Threading.Tasks;
using Gameplay.Player;
using UnityEngine;

namespace Gameplay.Drone
{
    public class PursuitMovementStrategy : IDroneMovementStrategy
    {
        [SerializeField] 
        private float baseSpeed = 15f;
        
        [SerializeField]
        private float turnRate = 1f;
        
        [SerializeField]
        private float momentumDrag = 0.2f;

        private Transform droneTransform;
        private Transform target;
        private Vector3 currentLocation;
        private Vector3 momentum;
        
        public void Initialise(DroneMovementBehaviour drone)
        {
            droneTransform = drone.transform;
            currentLocation = droneTransform.position;
            momentum = Vector3.zero;
            
            FetchTargetAsync().Forget();
        }

        private async UniTask FetchTargetAsync()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);
            target = PlayerAccessService.Instance.PlayerTransform;
        }

        public void Update()
        {
            if (target == null) return;
            
            // resync if we're off (might happen at end of transition)
            if ((currentLocation - droneTransform.position).sqrMagnitude > 0.01f)
            {
                currentLocation = droneTransform.position;
            }
    
            var desiredDirection = (target.position - currentLocation).normalized;
            var desiredVelocity = desiredDirection * baseSpeed;
            var turnForce = (desiredVelocity - momentum) * turnRate;
            
            momentum += turnForce * Time.deltaTime;
            momentum *= 1f - momentumDrag * Time.deltaTime;
            
            currentLocation += momentum * Time.deltaTime;
        }

        public Vector3 GetPosition()
        {
            return currentLocation;
        }

        public Vector3 GetVelocity()
        {
            return momentum;
        }

#if UNITY_EDITOR
        public void DrawGizmos() {}
#endif
    }
}