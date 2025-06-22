using System;
using Audio;
using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Core;
using Gameplay.Ghosts;
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

        private float ghostDisplayTime;
        
        public event Action<Vector2, float> OnVictorySequenceStart;
        public event Action<float> OnVictorySequenceFinish;
        public event Action OnBeatRainbowLeaderboardGhost;

        private void Awake()
        {
            if (SceneLoader.Instance.SceneLoadContext != null && 
                SceneLoader.Instance.SceneLoadContext.TryGetCustomData(GhostRunner.GHOST_DATA_KEY, out GhostContext ghostContext) &&
                ghostContext.GhostRun != null)
            {
                ghostDisplayTime = ghostContext.DisplayTime;
            }
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

            var finalTime = timerBehaviour.TimeElapsed;
            
            GameLogger.Log($"Unpaused realtime for completion was {timerBehaviour.RealtimeElapsed}s.", this);

            if (finalTime < ghostDisplayTime && 
                SceneLoader.Instance.CurrentSceneConfig.LevelConfig.GetTimeRanking(ghostDisplayTime) is TimeRanking.Rainbow)
            {
                OnBeatRainbowLeaderboardGhost?.Invoke();
            }

            OnVictorySequenceFinish?.Invoke(finalTime);
        }
    }
}