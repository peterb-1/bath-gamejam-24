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
        
        private PlayerMovementBehaviour playerMovementBehaviour;
        private Transform playerTransform;

        private float bounceCooldownTimer;
        private float angle;
        
        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerMovementBehaviour = PlayerAccessService.Instance.PlayerMovementBehaviour;
            playerTransform = PlayerAccessService.Instance.PlayerTransform;
            
            // angle of 0 represents straight upwards
            angle = transform.eulerAngles.z;
        }

        void Update()
        {
            if (bounceCooldownTimer > 0) bounceCooldownTimer -= Time.deltaTime;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (bounceCooldownTimer <= 0)
            {
                playerMovementBehaviour.PerformSpringJump(angle, minBounce, verticalDamping);
                bounceCooldownTimer = bounceCooldown;
            }
        }
    }

}
