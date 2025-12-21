using System;
using Cysharp.Threading.Tasks;
using Gameplay.Player;
using UnityEngine;

namespace Gameplay.Boss
{
    public class BossHitboxBehaviour : MonoBehaviour
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
        private BossMovementBehaviour bossMovementBehaviour;

        [SerializeField]
        private float hitboxDisableTime;

        [SerializeField]
        private float timeSlowDuration;

        [SerializeField]
        private Vector2 bossHitForce;
        
        private PlayerMovementBehaviour playerMovementBehaviour;
        private PlayerDeathBehaviour playerDeathBehaviour;

        private int health;
        private float hitboxDisableCountdown;
        
        private static readonly int Died = Animator.StringToHash("died");
        
        public static event Action<float> OnBossHit;

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerMovementBehaviour = PlayerAccessService.Instance.PlayerMovementBehaviour;
            playerDeathBehaviour = PlayerAccessService.Instance.PlayerDeathBehaviour;
            health = bossMovementBehaviour.GetMaxHealth();
            hitboxDisableCountdown = 0f;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((playerLayers.value & (1 << other.gameObject.layer)) != 0)
            {
                if (bossMovementBehaviour.IsDamageable() && playerMovementBehaviour.CanDashHit)
                {
                    HandleBossTakeDamage();
                }
                
                else if (hitboxDisableCountdown <= 0f && !playerMovementBehaviour.CanDashHit)
                {
                    HandleBossKilledPlayer();
                }
            }
        }
        
        private void HandleBossKilledPlayer()
        {
            playerDeathBehaviour.KillPlayer(PlayerDeathSource.Drone);
        }

        private void HandleBossTakeDamage()
        {
            if (health > 1)
            {
                health--;
                bossMovementBehaviour.IncrementProgress();
                hitboxDisableCountdown = hitboxDisableTime;
                
                playerMovementBehaviour.HandleBossHit(bossHitForce);
                OnBossHit?.Invoke(timeSlowDuration);
            }
            else
            {
                Debug.Log("Boss killed");
            }
        }

        private void Update()
        {
            hitboxDisableCountdown -= Time.deltaTime;
        }
    }
}