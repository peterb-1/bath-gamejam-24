using System;
using Audio;
using Cysharp.Threading.Tasks;
using Gameplay.Player;
using UnityEngine;
using Utils;

namespace Gameplay.Drone
{
    public class DroneHitboxBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private Animator droneAnimator;
        
        [SerializeField]
        private Collider2D droneCollider;
    
        [SerializeField] 
        private Rigidbody2D rigidBody;
        
        [SerializeField] 
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private LayerMask playerLayers;

        [SerializeField] 
        private Vector2 headAngleRange;

        [SerializeField] 
        private Vector2 deathDirectionStrength;

        [SerializeField] 
        private AnimationCurve fadeCurve;

        [SerializeField] 
        private float fadeDuration;

        private PlayerMovementBehaviour playerMovementBehaviour;
        private PlayerDeathBehaviour playerDeathBehaviour;
        
        private static readonly int Died = Animator.StringToHash("died");

        public event Action OnDroneKilled;

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerMovementBehaviour = PlayerAccessService.Instance.PlayerMovementBehaviour;
            playerDeathBehaviour = PlayerAccessService.Instance.PlayerDeathBehaviour;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((playerLayers.value & (1 << other.gameObject.layer)) != 0)
            {
                var direction = other.bounds.center.xy() - droneCollider.bounds.center.xy();
                var angle = Vector2.SignedAngle(transform.right.xy(), direction);

                if (angle > headAngleRange.x && angle < headAngleRange.y)
                {
                    HandlePlayerKilledDrone(direction);
                }
                else
                {
                    HandleDroneKilledPlayer();
                }
            }
        }

        private void HandlePlayerKilledDrone(Vector2 direction)
        {
            OnDroneKilled?.Invoke();
            
            playerMovementBehaviour.PerformHeadJump();
            
            AudioManager.Instance.Play(AudioClipIdentifier.DroneDeath);

            direction.Normalize();
            
            rigidBody.simulated = true;
            rigidBody.linearVelocity = -direction * deathDirectionStrength;
            rigidBody.gravityScale = 1f;
            rigidBody.constraints = RigidbodyConstraints2D.None;
            
            droneCollider.enabled = false;
            droneAnimator.SetTrigger(Died);
            
            RunFadeAsync().Forget();
        }
        
        private void HandleDroneKilledPlayer()
        {
            playerDeathBehaviour.KillPlayer();
        }

        private async UniTask RunFadeAsync()
        {
            var timeElapsed = 0f;
            var startColour = spriteRenderer.color;

            while (timeElapsed < fadeDuration)
            {
                var lerp = fadeCurve.Evaluate(timeElapsed / fadeDuration);

                spriteRenderer.color = lerp * startColour + (1f - lerp) * Color.clear;
                
                await UniTask.Yield();

                timeElapsed += Time.deltaTime;
            }
            
            spriteRenderer.color = Color.clear;

            rigidBody.simulated = false;
        }
    }
}