using Cysharp.Threading.Tasks;
using Gameplay.Core;
using Gameplay.Player;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Gameplay.Camera
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] 
        private Vector3 followOffset;

        [SerializeField] 
        private Vector2 minLookaheadVelocity;
        
        [SerializeField] 
        private Vector2 maxLookaheadVelocity;
        
        [SerializeField] 
        private Vector2 lookaheadStrength;
        
        [SerializeField]
        private float smoothTime;
        
        [SerializeField]
        private float lookaheadSmoothTime;

        [SerializeField] 
        private float velocityShakeThreshold;
        
        [SerializeField] 
        private float velocityShakeMultiplier;
        
        [SerializeField] 
        private float maxShakeStrength;

        private PlayerMovementBehaviour playerMovementBehaviour;
        private PlayerDeathBehaviour playerDeathBehaviour;
        private Transform target;
        private Vector3 deathPosition;
        private Vector3 velocity;
        private Vector2 currentLookahead;
        private Vector2 lookaheadVelocity;
        private bool useDeathPosition;

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);
            
            playerMovementBehaviour = PlayerAccessService.Instance.PlayerMovementBehaviour;
            playerDeathBehaviour = PlayerAccessService.Instance.PlayerDeathBehaviour;
            target = PlayerAccessService.Instance.PlayerTransform;
            
            velocity = Vector3.zero;
            lookaheadVelocity = Vector2.zero;
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
            if (useDeathPosition) return deathPosition;
            
            var playerVelocity = playerMovementBehaviour.Velocity;
            
            var signVector = new Vector2(Mathf.Sign(playerVelocity.x), Mathf.Sign(playerVelocity.y));
            var absoluteVelocity = new Vector2(Mathf.Abs(playerVelocity.x), Mathf.Abs(playerVelocity.y));
            var normalisedLookahead = new Vector2(
                Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(minLookaheadVelocity.x, maxLookaheadVelocity.x, absoluteVelocity.x)),
                Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(minLookaheadVelocity.y, maxLookaheadVelocity.y, absoluteVelocity.y)));

            var targetLookahead = normalisedLookahead * signVector * lookaheadStrength;
            var smoothedLookahead = Vector2.SmoothDamp(currentLookahead, targetLookahead, ref lookaheadVelocity, lookaheadSmoothTime);
            var targetPosition = target.position.xy() + smoothedLookahead;

            currentLookahead = smoothedLookahead;
            
            return (Vector3) targetPosition + followOffset;
        }

        private void OnDestroy()
        {
            playerDeathBehaviour.OnDeathSequenceStart -= HandleDeathSequenceStart;
        }
    }
}
