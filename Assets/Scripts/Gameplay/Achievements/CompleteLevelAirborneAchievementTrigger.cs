using Cysharp.Threading.Tasks;
using Gameplay.Player;

namespace Gameplay.Achievements
{
    public class CompleteLevelAirborneAchievementTrigger : AbstractAchievementTrigger
    {
        private PlayerMovementBehaviour playerMovementBehaviour;
        private PlayerVictoryBehaviour playerVictoryBehaviour;

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerMovementBehaviour = PlayerAccessService.Instance.PlayerMovementBehaviour;
            playerMovementBehaviour.OnLanded += HandleLanded;
                
            playerVictoryBehaviour = PlayerAccessService.Instance.PlayerVictoryBehaviour;
            playerVictoryBehaviour.OnVictorySequenceFinish += HandleVictorySequenceFinish;
        }

        private void HandleLanded()
        {
            playerMovementBehaviour.OnLanded -= HandleLanded;
            playerVictoryBehaviour.OnVictorySequenceFinish -= HandleVictorySequenceFinish;
        }

        private void HandleVictorySequenceFinish(float _1, bool _2)
        {
            TriggerAchievement();
        }

        private void OnDestroy()
        {
            if (playerMovementBehaviour != null)
            {
                playerMovementBehaviour.OnLanded -= HandleLanded;
            }
            
            if (playerVictoryBehaviour != null)
            {
                playerVictoryBehaviour.OnVictorySequenceFinish -= HandleVictorySequenceFinish;
            }
        }
    }
}