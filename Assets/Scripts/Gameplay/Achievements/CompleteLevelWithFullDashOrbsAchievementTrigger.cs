using Cysharp.Threading.Tasks;
using Gameplay.Dash;
using Gameplay.Player;

namespace Gameplay.Achievements
{
    public class CompleteLevelWithFullDashOrbsAchievementTrigger : AbstractAchievementTrigger
    {
        private PlayerVictoryBehaviour playerVictoryBehaviour;

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);
            
            playerVictoryBehaviour = PlayerAccessService.Instance.PlayerVictoryBehaviour;
            playerVictoryBehaviour.OnVictorySequenceFinish += HandleVictorySequenceFinish;
        }

        private void HandleVictorySequenceFinish(float _)
        {
            if (DashTrackerService.Instance.HasFullOrbCapacity)
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