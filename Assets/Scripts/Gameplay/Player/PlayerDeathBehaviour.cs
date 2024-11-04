using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Player
{
    public class PlayerDeathBehaviour : MonoBehaviour
    {
        [SerializeField]
        private float deathSequenceDuration;
        
        [SerializeField] 
        private LayerMask deathLayers;

        [SerializeField] 
        private BoxCollider2D playerHitbox;
        
        [SerializeField] 
        private SpriteRenderer playerSpriteRenderer;

        [SerializeField] 
        private ParticleSystem deathParticles;

        public event Action OnDeathSequenceStart;
        public event Action OnDeathSequenceFinish;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((deathLayers.value & (1 << other.gameObject.layer)) != 0)
            {
                RunDeathSequenceAsync().Forget();
            }
        }

        private async UniTask RunDeathSequenceAsync()
        {
            OnDeathSequenceStart?.Invoke();

            playerHitbox.enabled = false;
            playerSpriteRenderer.enabled = false;
            
            deathParticles.gameObject.SetActive(true);
            deathParticles.Play();

            await UniTask.Delay(TimeSpan.FromSeconds(deathSequenceDuration));
            
            playerHitbox.enabled = true;
            playerSpriteRenderer.enabled = true;
            
            deathParticles.gameObject.SetActive(false);
            deathParticles.Stop();

            OnDeathSequenceFinish?.Invoke();
        }
    }
}
