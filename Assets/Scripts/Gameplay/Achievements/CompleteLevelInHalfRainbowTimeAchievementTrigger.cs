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

        private void HandleVictorySequenceFinish(int milliseconds, bool _)
        {
            if (2f * milliseconds <= SceneLoader.Instance.CurrentSceneConfig.LevelConfig.RainbowMilliseconds)
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