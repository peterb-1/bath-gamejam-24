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

        private PlayerMovementBehaviour playerMovementBehaviour;
        private PlayerDeathBehaviour playerDeathBehaviour;
        
        private static readonly int Died = Animator.StringToHash("died");
        private float alpha = 1.0f;

        public event Action OnDroneKilled;

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerMovementBehaviour = PlayerAccessService.Instance.PlayerMovementBehaviour;
            playerDeathBehaviour = PlayerAccessService.Instance.PlayerDeathBehaviour;
        }
        
        private void Update()
        {
            if (alpha is < 1.0f and > 0.0f)
            {
                alpha -= 0.00005f / Time.deltaTime;
                var temp = spriteRenderer.color;
                temp.a = alpha;
                spriteRenderer.color = temp;
            }
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
            
            droneCollider.enabled = false;
            
            AudioManager.Instance.Play(AudioClipIdentifier.DroneDeath);

            direction.Normalize();
            
            rigidBody.linearVelocity = -direction * deathDirectionStrength;
            rigidBody.gravityScale = 1f;
            rigidBody.constraints = RigidbodyConstraints2D.None;
            
            droneAnimator.SetTrigger(Died);
            alpha = 0.99f;
        }
        
        private void HandleDroneKilledPlayer()
        {
            playerDeathBehaviour.KillPlayer();
        }
    }
}