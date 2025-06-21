using System;
using Audio;
using Core;
using Core.Saving;
using Cysharp.Threading.Tasks;
using Gameplay.Environment;
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
                if (other.GetComponentInParent<Laser>() != null)
                {
                    KillPlayer(PlayerDeathSource.Laser);
                }
                else if (other.GetComponentInParent<Building>() != null)
                {
                    KillPlayer(PlayerDeathSource.Building);
                }
                else
                {
                    KillPlayer(PlayerDeathSource.Cloud);
                }
            }
        }

        public void KillPlayer(PlayerDeathSource source)
        {
            if (!IsAlive || SceneLoader.Instance.IsLoading) return;
            
            IsAlive = false;

            var statType = source switch
            {
                PlayerDeathSource.Building => StatType.BuildingDeaths,
                PlayerDeathSource.Cloud => StatType.CloudDeaths,
                PlayerDeathSource.Drone => StatType.DroneDeaths,
                PlayerDeathSource.Laser => StatType.LaserDeaths,
                _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
            };
            
            SaveManager.Instance.SaveData.StatsData.AddToStat(statType, 1);
            
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
