using Core.Saving;
using Cysharp.Threading.Tasks;
using Gameplay.Trails;

namespace Gameplay.Achievements
{
    public class ChangeTrailAchievementTrigger : AbstractAchievementTrigger
    {
        private async void Awake()
        {
            await UniTask.WaitUntil(() => SaveManager.IsReady);

            SaveManager.Instance.SaveData.PreferenceData.OnTrailSet += HandleTrailSet;
        }

        private void HandleTrailSet(Trail trail)
        {
            if (SaveManager.Instance.SaveData.AchievementsData.TryGetAchievementForTrail(trail, out _))
            {
                TriggerAchievement();
                
                SaveManager.Instance.SaveData.PreferenceData.OnTrailSet -= HandleTrailSet;
            }
        }
        
        private void OnDestroy()
        {
            SaveManager.Instance.SaveData.PreferenceData.OnTrailSet -= HandleTrailSet;
        }
    }
}