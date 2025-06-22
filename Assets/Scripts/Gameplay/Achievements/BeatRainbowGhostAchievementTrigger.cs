using Cysharp.Threading.Tasks;
using Gameplay.Player;

namespace Gameplay.Achievements
{
    public class BeatRainbowGhostAchievementTrigger : AbstractAchievementTrigger
    {
        private PlayerVictoryBehaviour playerVictoryBehaviour;
        
        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerVictoryBehaviour = PlayerAccessService.Instance.PlayerVictoryBehaviour;

            playerVictoryBehaviour.OnBeatRainbowLeaderboardGhost += HandleBeatRainbowLeaderboardGhost;
        }

        private void HandleBeatRainbowLeaderboardGhost()
        {
            TriggerAchievement();
        }

        private void OnDestroy()
        {
            if (playerVictoryBehaviour != null)
            {
                playerVictoryBehaviour.OnBeatRainbowLeaderboardGhost -= HandleBeatRainbowLeaderboardGhost;
            }
        }
    }
}