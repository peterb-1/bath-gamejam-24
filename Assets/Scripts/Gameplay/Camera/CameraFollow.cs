using Cysharp.Threading.Tasks;
using Gameplay.Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.Camera
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] 
        private Vector3 followOffset;
        
        [SerializeField]
        private float smoothTime;

        [SerializeField] 
        private float velocityShakeThreshold;
        
        [SerializeField] 
        private float velocityShakeMultiplier;
        
        [SerializeField] 
        private float maxShakeStrength;

        private PlayerDeathBehaviour playerDeathBehaviour;
        private Transform target;
        private Vector3 velocity;

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);
            
            target = PlayerAccessService.Instance.PlayerTransform;
            velocity = Vector3.zero;
            
            playerDeathBehaviour = PlayerAccessService.Instance.PlayerDeathBehaviour;
            playerDeathBehaviour.OnDeathSequenceFinish += HandleDeathSequenceFinish;
        }

        private void HandleDeathSequenceFinish()
        {
            SnapToTarget();
        }

        private void Update()
        {
            var velocityMagnitude = velocity.magnitude - velocityShakeThreshold;
            var shakeIntensity = Mathf.Clamp(velocityMagnitude * velocityShakeMultiplier, 0, maxShakeStrength);
            var shakeOffset = new Vector3(Random.Range(-1f, 1f) * shakeIntensity, Random.Range(-1f, 1f) * shakeIntensity, 0f);

            transform.position = Vector3.SmoothDamp(transform.position, GetTargetPosition() + shakeOffset, ref velocity, smoothTime);
        }

        private void SnapToTarget()
        {
            velocity = Vector3.zero;
            transform.position = GetTargetPosition();
        }

        private Vector3 GetTargetPosition()
        {
            var currentPosition = target.position;
            return new Vector3(currentPosition.x + followOffset.x, currentPosition.y + followOffset.y, followOffset.z);
        }

        private void OnDestroy()
        {
            playerDeathBehaviour.OnDeathSequenceFinish -= HandleDeathSequenceFinish;
        }
    }
}
