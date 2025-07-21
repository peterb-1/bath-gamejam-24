using Cysharp.Threading.Tasks;
using Gameplay.Colour;
using Gameplay.Player;

namespace Gameplay.Achievements
{
    public class CompleteLevelWithoutColourSwitchAchievementTrigger : AbstractAchievementTrigger
    {
        private PlayerVictoryBehaviour playerVictoryBehaviour;

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);
            
            playerVictoryBehaviour = PlayerAccessService.Instance.PlayerVictoryBehaviour;
            playerVictoryBehaviour.OnVictorySequenceFinish += HandleVictorySequenceFinish;

            ColourManager.OnColourChangeStarted += HandleColourChangeStarted;
        }

        private void HandleColourChangeStarted(ColourId _1, float _2)
        {
            playerVictoryBehaviour.OnVictorySequenceFinish -= HandleVictorySequenceFinish;
            
            ColourManager.OnColourChangeStarted -= HandleColourChangeStarted;
        }

        private void HandleVictorySequenceFinish(int _1, bool _2)
        {
            TriggerAchievement();
        }

        private void OnDestroy()
        {
            if (playerVictoryBehaviour != null)
            {
                playerVictoryBehaviour.OnVictorySequenceFinish -= HandleVictorySequenceFinish;
            }

            ColourManager.OnColourChangeStarted -= HandleColourChangeStarted;
        }
    }
}