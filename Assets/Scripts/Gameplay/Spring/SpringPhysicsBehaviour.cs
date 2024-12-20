using System;
using Cysharp.Threading.Tasks;
using Gameplay.Player;
using UnityEngine;

namespace Gameplay.Spring
{
    public class SpringPhysicsBehaviour : MonoBehaviour
    {
        [SerializeField]
        private float bounceCooldown;

        [SerializeField]
        private float minBounce;
        
        [SerializeField]
        private float verticalDamping;
        
        [SerializeField]
        private float timeSlowDuration;
        
        [SerializeField] 
        private float springCushionDuration;
        
        [SerializeField] 
        private float springCushionStrength;
        
        private PlayerMovementBehaviour playerMovementBehaviour;
        public static event Action<float> OnBounce;

        private float bounceCooldownTimer;
        private float angle;
        private float springCushionCountdown;
        private bool canSpringJump;
        
        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerMovementBehaviour = PlayerAccessService.Instance.PlayerMovementBehaviour;
            
            // angle of 0 represents straight upwards
            angle = transform.eulerAngles.z;
        }

        void Update()
        {
            if (bounceCooldownTimer > 0) bounceCooldownTimer -= Time.deltaTime;
            if (springCushionCountdown > 0)
            {
                springCushionCountdown -= Time.deltaTime;
                playerMovementBehaviour.PerformSpringSlowdown(springCushionStrength);
            }

            if (springCushionCountdown <= 0)
            {
                if (canSpringJump) InitiateSpringJump();
                canSpringJump = false;
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (bounceCooldownTimer > 0) return;
            springCushionCountdown = springCushionDuration;
            canSpringJump = true;
        }

        private void InitiateSpringJump()
        {
            if (bounceCooldownTimer <= 0)
            {
                playerMovementBehaviour.PerformSpringJump(angle, minBounce, verticalDamping);
                bounceCooldownTimer = bounceCooldown;
                OnBounce?.Invoke(timeSlowDuration);
            }
        }
    }

}
