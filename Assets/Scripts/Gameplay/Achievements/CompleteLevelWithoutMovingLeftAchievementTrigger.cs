using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Player;
using UnityEngine;

namespace Gameplay.Achievements
{
    public class CompleteLevelWithoutMovingLeftAchievementTrigger : AbstractAchievementTrigger
    {
        [SerializeField] 
        private SceneConfig sceneConfig;

        private PlayerMovementBehaviour playerMovementBehaviour;
        
        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerMovementBehaviour = PlayerAccessService.Instance.PlayerMovementBehaviour;

            playerMovementBehaviour.OnMissionCompleteWithoutMovingLeft += HandleMissionCompleteWithoutMovingLeft;
        }

        private void HandleMissionCompleteWithoutMovingLeft()
        {
            if (SceneLoader.Instance.CurrentSceneConfig.LevelConfig.Guid == sceneConfig.LevelConfig.Guid)
            {
                TriggerAchievement();
            }
        }

        private void OnDestroy()
        {
            if (playerMovementBehaviour != null)
            {
                playerMovementBehaviour.OnMissionCompleteWithoutMovingLeft -= HandleMissionCompleteWithoutMovingLeft;
            }
        }
    }
}