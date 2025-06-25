using System;
using Audio;
using Cysharp.Threading.Tasks;
using Hardware;
using UI;
using UnityEngine;
using Utils;

namespace Gameplay.Player
{
    public class PlayerVictoryBehaviour : MonoBehaviour
    {
        [SerializeField]
        private TimerBehaviour timerBehaviour;
        
        [SerializeField]
        private float victorySequenceDuration;
        
        [SerializeField] 
        private LayerMask victoryLayers;

        [SerializeField] 
        private Collider2D playerHitbox;

        [SerializeField] 
        private RumbleConfig victoryRumbleConfig;

        private bool hasFoundCollectible;

        public event Action<Vector2, float> OnVictorySequenceStart;
        public event Action<float, bool> OnVictorySequenceFinish;

        public void NotifyFoundCollectible()
        {
            hasFoundCollectible = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((victoryLayers.value & (1 << other.gameObject.layer)) != 0)
            {
                RunVictorySequenceAsync(other.transform.position).Forget();
            }
        }

        private async UniTask RunVictorySequenceAsync(Vector3 targetPosition)
        {
            GameLogger.Log("Level complete - running victory sequence", this);
            
            OnVictorySequenceStart?.Invoke(targetPosition, victorySequenceDuration);
            
            AudioManager.Instance.Play(AudioClipIdentifier.Victory);
            RumbleManager.Instance.Rumble(victoryRumbleConfig);

            playerHitbox.enabled = false;

            await UniTask.Delay(TimeSpan.FromSeconds(victorySequenceDuration));

            GameLogger.Log($"Unpaused realtime for completion was {timerBehaviour.RealtimeElapsed}s.", this);

            OnVictorySequenceFinish?.Invoke(timerBehaviour.TimeElapsed, hasFoundCollectible);
        }
    }
}