using Cysharp.Threading.Tasks;
using Gameplay.Player;

namespace Gameplay.Achievements
{
    public class DashIntoLaserAchievementTrigger : AbstractAchievementTrigger
    {
        private PlayerMovementBehaviour playerMovementBehaviour;
        
        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerMovementBehaviour = PlayerAccessService.Instance.PlayerMovementBehaviour;

            playerMovementBehaviour.OnPlayerDashedIntoLaser += HandlePlayerDashedIntoLaser;
        }

        private void HandlePlayerDashedIntoLaser()
        {
            TriggerAchievement();
        }

        private void OnDestroy()
        {
            if (playerMovementBehaviour != null)
            {
                playerMovementBehaviour.OnPlayerDashedIntoLaser -= HandlePlayerDashedIntoLaser;
            }
        }
    }
}