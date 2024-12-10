using Cysharp.Threading.Tasks;
using Gameplay.Core;
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
        private Vector3 deathPosition;
        private Vector3 velocity;
        private bool useDeathPosition;

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);
            
            playerDeathBehaviour = PlayerAccessService.Instance.PlayerDeathBehaviour;
            target = PlayerAccessService.Instance.PlayerTransform;
            velocity = Vector3.zero;
            useDeathPosition = false;

            playerDeathBehaviour.OnDeathSequenceStart += HandleDeathSequenceStart;
        }

        private void HandleDeathSequenceStart()
        {
            deathPosition = transform.position;
            useDeathPosition = true;
        }

        private void Update()
        {
            if (Time.deltaTime == 0f) return;
                
            var velocityMagnitude = velocity.magnitude - velocityShakeThreshold;
            var shakeIntensity = Mathf.Clamp(velocityMagnitude * velocityShakeMultiplier, 0, maxShakeStrength) / Time.deltaTime;
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
            var currentPosition = useDeathPosition ? deathPosition : target.position;
            return new Vector3(currentPosition.x + followOffset.x, currentPosition.y + followOffset.y, followOffset.z);
        }

        private void OnDestroy()
        {
            playerDeathBehaviour.OnDeathSequenceStart -= HandleDeathSequenceStart;
        }
    }
}
