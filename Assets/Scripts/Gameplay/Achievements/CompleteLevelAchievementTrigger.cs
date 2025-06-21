using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Player;
using UnityEngine;
using Utils;

namespace Gameplay.Achievements
{
    public class CompleteLevelAchievementTrigger : AbstractAchievementTrigger
    {
        [SerializeField]
        private SceneConfig sceneConfig;

        private PlayerVictoryBehaviour playerVictoryBehaviour;

        private async void Awake()
        {
            if (!sceneConfig.IsLevelScene)
            {
                GameLogger.LogError($"{name} has a non-level SceneConfig set - it will not work", this);
            }

            if (!SceneLoader.Instance.CurrentSceneConfig.IsLevelScene ||
                SceneLoader.Instance.CurrentSceneConfig.LevelConfig.Guid != sceneConfig.LevelConfig.Guid) return;
            
            await UniTask.WaitUntil(PlayerAccessService.IsReady);
            
            playerVictoryBehaviour = PlayerAccessService.Instance.PlayerVictoryBehaviour;

            playerVictoryBehaviour.OnVictorySequenceStart += HandleVictorySequenceStart;
        }

        private void HandleVictorySequenceStart(Vector2 _1, float _2)
        {
            TriggerAchievement();
        }
        
        private void OnDestroy()
        {
            if (playerVictoryBehaviour != null)
            {
                playerVictoryBehaviour.OnVictorySequenceStart -= HandleVictorySequenceStart;
            }
        }
    }
}