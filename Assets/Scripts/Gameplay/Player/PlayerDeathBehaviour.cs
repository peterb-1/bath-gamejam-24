using System;
using Audio;
using Core;
using Cysharp.Threading.Tasks;
using Hardware;
using UnityEngine;
using Utils;

namespace Gameplay.Player
{
    public class PlayerDeathBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private Collider2D playerHitbox;
        
        [SerializeField] 
        private SpriteRenderer playerSpriteRenderer;

        [SerializeField] 
        private ParticleSystem deathParticles;
        
        [SerializeField] 
        private RumbleConfig deathRumbleConfig;
        
        [SerializeField] 
        private LayerMask deathLayers;
        
        [SerializeField]
        private float deathSequenceDuration;

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
                KillPlayer();
            }
        }

        public void KillPlayer()
        {
            if (!IsAlive || SceneLoader.Instance.IsLoading) return;
            
            IsAlive = false;
            
            GameLogger.Log("Player died - running death sequence", this);
            
            RunDeathSequenceAsync().Forget();
        }

        private async UniTask RunDeathSequenceAsync()
        {
            OnDeathSequenceStart?.Invoke();
            
            AudioManager.Instance.Play(AudioClipIdentifier.Death);
            AudioManager.Instance.Stop(AudioClipIdentifier.ColourSwitch);
            
            RumbleManager.Instance.Rumble(deathRumbleConfig);

            playerHitbox.enabled = false;
            playerSpriteRenderer.enabled = false;
            
            deathParticles.gameObject.SetActive(true);
            deathParticles.Play();

            await UniTask.Delay(TimeSpan.FromSeconds(deathSequenceDuration));
            
            SceneLoader.Instance.ReloadCurrentScene();
        }
    }
}
