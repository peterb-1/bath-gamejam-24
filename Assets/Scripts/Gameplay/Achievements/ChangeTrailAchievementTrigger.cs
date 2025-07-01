using Core.Saving;
using Cysharp.Threading.Tasks;

namespace Gameplay.Achievements
{
    public class ChangeTrailAchievementTrigger : AbstractAchievementTrigger
    {
        private async void Awake()
        {
            await UniTask.WaitUntil(() => SaveManager.IsReady);

            SaveManager.Instance.SaveData.PreferenceData.OnSettingChanged += HandleSettingChanged;
        }

        private void HandleSettingChanged(SettingId settingId, object trailObject)
        {
            if (settingId is SettingId.Trail && trailObject is string trailGuid)
            {
                if (SaveManager.Instance.SaveData.AchievementsData.TryGetAchievementForTrail(trailGuid, out _))
                {
                    TriggerAchievement();
                
                    SaveManager.Instance.SaveData.PreferenceData.OnSettingChanged -= HandleSettingChanged;
                }
            }
        }
        
        private void OnDestroy()
        {
            SaveManager.Instance.SaveData.PreferenceData.OnSettingChanged -= HandleSettingChanged;
        }
    }
}