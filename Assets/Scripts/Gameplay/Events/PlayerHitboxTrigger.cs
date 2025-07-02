using UnityEngine;

namespace Gameplay.Events
{
    public class PlayerHitboxTrigger : AbstractEventTrigger
    {
        [SerializeField]
        private LayerMask playerLayers;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((playerLayers.value & (1 << other.gameObject.layer)) != 0)
            {
                TriggerSequence();
            }
        }
    }
}