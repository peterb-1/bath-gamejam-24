using Cysharp.Threading.Tasks;
using Gameplay.Player;
using UnityEngine;

namespace Gameplay.Events
{
    public class GroundedPlayerHitboxTrigger : AbstractEventTrigger
    {
        [SerializeField]
        private LayerMask playerLayers;
        
        private PlayerMovementBehaviour playerMovementBehaviour;
        
        public override async void Awake()
        {
            base.Awake();
            
            await UniTask.WaitUntil(PlayerAccessService.IsReady);
            playerMovementBehaviour = PlayerAccessService.Instance.PlayerMovementBehaviour;
        }
        
        private void OnTriggerStay2D(Collider2D other)
        {
            if (((playerLayers.value & (1 << other.gameObject.layer)) != 0) && playerMovementBehaviour.IsOnGround)
            {
                TriggerSequence();
            }
        }
    }
}