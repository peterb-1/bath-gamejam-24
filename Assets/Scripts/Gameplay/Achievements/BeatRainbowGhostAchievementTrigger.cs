using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Core;
using Gameplay.Ghosts;
using Gameplay.Player;
using UI;

namespace Gameplay.Achievements
{
    public class BeatRainbowGhostAchievementTrigger : AbstractAchievementTrigger
    {
        private PlayerVictoryBehaviour playerVictoryBehaviour;
        private int ghostDisplayMilliseconds;
        
        private async void Awake()
        {
            if (SceneLoader.Instance.SceneLoadContext != null && 
                SceneLoader.Instance.SceneLoadContext.TryGetCustomData(GhostRunner.GHOST_DATA_KEY, out GhostContext ghostContext) &&
                ghostContext.GhostRun != null)
            {
                ghostDisplayMilliseconds = ghostContext.DisplayMilliseconds;
            }
            
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerVictoryBehaviour = PlayerAccessService.Instance.PlayerVictoryBehaviour;

            playerVictoryBehaviour.OnVictorySequenceFinish += HandleVictorySequenceFinish;
        }

        private void HandleVictorySequenceFinish(int milliseconds, bool _)
        {
            if (milliseconds < ghostDisplayMilliseconds &&
                TimerBehaviour.GetFormattedTime(milliseconds) != TimerBehaviour.GetFormattedTime(ghostDisplayMilliseconds) &&
                SceneLoader.Instance.CurrentSceneConfig.LevelConfig.GetTimeRanking(ghostDisplayMilliseconds) is TimeRanking.Rainbow)
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