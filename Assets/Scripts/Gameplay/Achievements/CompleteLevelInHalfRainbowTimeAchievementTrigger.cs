using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Player;

namespace Gameplay.Achievements
{
    public class CompleteLevelInHalfRainbowTimeAchievementTrigger : AbstractAchievementTrigger
    {
        private PlayerVictoryBehaviour playerVictoryBehaviour;
        
        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerVictoryBehaviour = PlayerAccessService.Instance.PlayerVictoryBehaviour;

            playerVictoryBehaviour.OnVictorySequenceFinish += HandleVictorySequenceFinish;
        }

        private void HandleVictorySequenceFinish(float time)
        {
            if (2f * time <= SceneLoader.Instance.CurrentSceneConfig.LevelConfig.RainbowTime)
            {
                TriggerAchievement();
            }
        }

        private void OnDestroy()
        {
            if (playerVictoryBehaviour != null)
            {
                playerVictoryBehaviour.OnVictorySequenceFinish -= HandleVictorySequenceFinish;
            }
        }
    }
}