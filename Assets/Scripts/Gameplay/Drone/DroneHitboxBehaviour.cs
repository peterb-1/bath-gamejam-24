using System;
using Audio;
using Core.Saving;
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
        
        [field: SerializeField] 
        public float TimeBonusOnKilled { get; private set; }

        private PlayerMovementBehaviour playerMovementBehaviour;
        private PlayerDeathBehaviour playerDeathBehaviour;
        
        private static readonly int Died = Animator.StringToHash("died");
        private static readonly int Threshold = Shader.PropertyToID("_Threshold");

        public event Action<DroneHitboxBehaviour> OnDroneKilled;

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerMovementBehaviour = PlayerAccessService.Instance.PlayerMovementBehaviour;
            playerDeathBehaviour = PlayerAccessService.Instance.PlayerDeathBehaviour;
            
            DroneTrackerService.RegisterDrone(this);
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
            OnDroneKilled?.Invoke(this);
            
            playerMovementBehaviour.PerformHeadJump();
            
            AudioManager.Instance.Play(AudioClipIdentifier.DroneDeath);
            SaveManager.Instance.SaveData.StatsData.AddToStat(StatType.DronesKilled, 1);

            direction.Normalize();
            
            rigidBody.simulated = true;
            rigidBody.linearVelocity = -direction * deathDirectionStrength;
            rigidBody.gravityScale = 1f;
            rigidBody.constraints = RigidbodyConstraints2D.None;
            
            droneCollider.enabled = false;
            droneAnimator.SetTrigger(Died);
            
            RunDissolveAsync().Forget();
        }
        
        private void HandleDroneKilledPlayer()
        {
            playerDeathBehaviour.KillPlayer(PlayerDeathSource.Drone);
        }

        private async UniTask RunDissolveAsync()
        {
            var timeElapsed = 0f;

            while (timeElapsed < fadeDuration)
            {
                var lerp = fadeCurve.Evaluate(timeElapsed / fadeDuration);

                spriteRenderer.material.SetFloat(Threshold, lerp);
                
                await UniTask.Yield();

                timeElapsed += Time.deltaTime;
            }
            
            spriteRenderer.material.SetFloat(Threshold, 1f);

            rigidBody.simulated = false;
        }
    }
}