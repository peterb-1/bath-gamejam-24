using Cysharp.Threading.Tasks;
using Gameplay.Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.Camera
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] 
        private UnityEngine.Camera cameraToFollow;
        
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
        private float shakeSmoothTime;

        [SerializeField] 
        private float velocityShakeThreshold;
        
        [SerializeField] 
        private float velocityShakeMultiplier;
        
        [SerializeField] 
        private float maxShakeStrength;

        private CameraBorderZone cameraBorderZone;
        private PlayerMovementBehaviour playerMovementBehaviour;
        private PlayerVictoryBehaviour playerVictoryBehaviour;
        private PlayerDeathBehaviour playerDeathBehaviour;
        private Transform target;
        
        private Vector3 positionOverride;
        private Vector3 velocity;
        private Vector3 rawPosition;
        private Vector3 shakeVelocity;
        private Vector3 shakePosition;
        private Vector2 currentLookahead;
        private Vector2 lookaheadVelocity;
        
        private bool shouldOverridePosition;
        private bool shouldUseLookahead;

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);
            
            playerMovementBehaviour = PlayerAccessService.Instance.PlayerMovementBehaviour;
            playerVictoryBehaviour = PlayerAccessService.Instance.PlayerVictoryBehaviour;
            playerDeathBehaviour = PlayerAccessService.Instance.PlayerDeathBehaviour;
            target = PlayerAccessService.Instance.PlayerTransform;
            
            velocity = Vector3.zero;
            shakeVelocity = Vector3.zero;
            shakePosition = Vector3.zero;
            lookaheadVelocity = Vector2.zero;
            shouldOverridePosition = false;
            shouldUseLookahead = true;
            rawPosition = transform.position;

            playerVictoryBehaviour.OnVictorySequenceStart += HandleVictorySequenceStart;
            playerDeathBehaviour.OnDeathSequenceStart += HandleDeathSequenceStart;
        }

        private void HandleVictorySequenceStart(Vector2 position, float _)
        {
            shouldUseLookahead = false;
        }

        private void HandleDeathSequenceStart()
        {
            positionOverride = transform.position;
            shouldOverridePosition = true;
        }
        
        public void RegisterBorderZone(CameraBorderZone borderZone)
        {
            cameraBorderZone = borderZone;
        }

        private void Update()
        {
            if (Time.deltaTime == 0f) return;
                
            var velocityMagnitude = velocity.magnitude - velocityShakeThreshold;
            var shakeIntensity = Mathf.Clamp(velocityMagnitude * velocityShakeMultiplier, 0, maxShakeStrength) / Time.deltaTime;
            var shakeOffset = new Vector3(Random.Range(-1f, 1f) * shakeIntensity, Random.Range(-1f, 1f) * shakeIntensity, 0f);

            rawPosition = Vector3.SmoothDamp(rawPosition, GetTargetPosition(), ref velocity, smoothTime);
            shakePosition = Vector3.SmoothDamp(shakePosition, shakePosition + shakeOffset, ref shakeVelocity, shakeSmoothTime);

            transform.position = rawPosition + shakePosition;
        }

        private Vector3 GetTargetPosition()
        {
            if (shouldOverridePosition)
            {
                return positionOverride;
            }

            var targetPosition = target.position + followOffset;

            if (shouldUseLookahead)
            {
                var playerVelocity = playerMovementBehaviour.Velocity;
            
                var signVector = new Vector2(Mathf.Sign(playerVelocity.x), Mathf.Sign(playerVelocity.y));
                var absoluteVelocity = new Vector2(Mathf.Abs(playerVelocity.x), Mathf.Abs(playerVelocity.y));
                var normalisedLookahead = new Vector2(
                    Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(minLookaheadVelocity.x, maxLookaheadVelocity.x, absoluteVelocity.x)),
                    Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(minLookaheadVelocity.y, maxLookaheadVelocity.y, absoluteVelocity.y)));

                var targetLookahead = normalisedLookahead * signVector * lookaheadStrength;
                var smoothedLookahead = Vector2.SmoothDamp(currentLookahead, targetLookahead, ref lookaheadVelocity, lookaheadSmoothTime);
                
                currentLookahead = smoothedLookahead;
                
                targetPosition = target.position + (Vector3) smoothedLookahead;
            }
            
            if (cameraBorderZone != null)
            {
                targetPosition = cameraBorderZone.SoftClampPosition(cameraToFollow, targetPosition);
            }

            return targetPosition + followOffset;
        }

        private void OnDestroy()
        {
            playerVictoryBehaviour.OnVictorySequenceStart -= HandleVictorySequenceStart;
            playerDeathBehaviour.OnDeathSequenceStart -= HandleDeathSequenceStart;
        }
    }
}
