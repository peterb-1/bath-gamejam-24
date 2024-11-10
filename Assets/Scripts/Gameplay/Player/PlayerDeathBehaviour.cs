using System;
using Audio;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils;

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
        
        public bool IsAlive { get; private set; }

        public event Action OnDeathSequenceStart;

        private void Awake()
        {
            IsAlive = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((deathLayers.value & (1 << other.gameObject.layer)) != 0)
            {
                RunDeathSequenceAsync().Forget();
            }
        }

        private async UniTask RunDeathSequenceAsync()
        {
            GameLogger.Log("Player died - running death sequence", this);
            
            IsAlive = false;
            
            OnDeathSequenceStart?.Invoke();
            
            AudioManager.Instance.Play(AudioClipIdentifier.Death);
            AudioManager.Instance.Stop(AudioClipIdentifier.ColourSwitch);

            playerHitbox.enabled = false;
            playerSpriteRenderer.enabled = false;
            
            deathParticles.gameObject.SetActive(true);
            deathParticles.Play();

            await UniTask.Delay(TimeSpan.FromSeconds(deathSequenceDuration));
            
            SceneLoader.Instance.ReloadCurrentScene();
        }
    }
}
