using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Core;
using Gameplay.Ghosts;
using Gameplay.Player;

namespace Gameplay.Achievements
{
    public class BeatRainbowGhostAchievementTrigger : AbstractAchievementTrigger
    {
        private PlayerVictoryBehaviour playerVictoryBehaviour;
        private float ghostDisplayTime;
        
        private async void Awake()
        {
            if (SceneLoader.Instance.SceneLoadContext != null && 
                SceneLoader.Instance.SceneLoadContext.TryGetCustomData(GhostRunner.GHOST_DATA_KEY, out GhostContext ghostContext) &&
                ghostContext.GhostRun != null)
            {
                ghostDisplayTime = ghostContext.DisplayTime;
            }
            
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerVictoryBehaviour = PlayerAccessService.Instance.PlayerVictoryBehaviour;

            playerVictoryBehaviour.OnVictorySequenceFinish += HandleVictorySequenceFinish;
        }

        private void HandleVictorySequenceFinish(float time, bool _)
        {
            if (time < ghostDisplayTime && 
                SceneLoader.Instance.CurrentSceneConfig.LevelConfig.GetTimeRanking(ghostDisplayTime) is TimeRanking.Rainbow)
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