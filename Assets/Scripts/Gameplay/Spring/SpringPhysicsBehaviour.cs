using System;
using Cysharp.Threading.Tasks;
using Gameplay.Player;
using UnityEngine;

namespace Gameplay.Spring
{
    public class SpringPhysicsBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private LayerMask playerLayers;

        [SerializeField]
        private Vector2 minBounce;
        
        [SerializeField]
        private float bounceCooldown;

        [SerializeField]
        private float verticalDamping;
        
        [SerializeField]
        private float timeSlowDuration;
        
        private PlayerMovementBehaviour playerMovementBehaviour;

        private Vector2 minDirectionalBounce;
        private float bounceCooldownTimer;
        private float angleRadians;
        private float angle;
        private float springCushionCountdown;
        private bool canSpringJump;
        
        public static event Action<float> OnBounce;
        
        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerMovementBehaviour = PlayerAccessService.Instance.PlayerMovementBehaviour;
            
            // angle of 0 represents straight upwards
            angleRadians = Mathf.Deg2Rad * transform.eulerAngles.z;
            minDirectionalBounce = new Vector2(-Mathf.Sin(angleRadians), Mathf.Cos(angleRadians)) * minBounce;
        }

        private void Update()
        {
            if (bounceCooldownTimer > 0) bounceCooldownTimer -= Time.deltaTime;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((playerLayers.value & (1 << other.gameObject.layer)) != 0)
            {
                if (!(bounceCooldownTimer <= 0)) return;
                
                playerMovementBehaviour.PerformSpringJump(angle, minDirectionalBounce, verticalDamping);
                bounceCooldownTimer = bounceCooldown;
                
                OnBounce?.Invoke(timeSlowDuration);
            }
        }
    }
}
