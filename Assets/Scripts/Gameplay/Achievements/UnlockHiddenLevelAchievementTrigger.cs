using Core;
using Core.Saving;
using Cysharp.Threading.Tasks;
using Gameplay.Core;

namespace Gameplay.Achievements
{
    public class UnlockHiddenLevelAchievementTrigger : AbstractAchievementTrigger
    {
        private async void Awake()
        {
            await UniTask.WaitUntil(() => SaveManager.IsReady);

            foreach (var sceneConfig in SceneLoader.Instance.SceneConfigs)
            {
                if (!sceneConfig.IsLevelScene) continue;
                
                if (SaveManager.Instance.SaveData.CampaignData.TryGetLevelData(sceneConfig.LevelConfig, out var levelData) && 
                    levelData.IsUnlocked && 
                    sceneConfig.LevelConfig.LevelType is LevelType.Hidden)
                {
                    TriggerAchievement();
                }
            }
        }
    }
}