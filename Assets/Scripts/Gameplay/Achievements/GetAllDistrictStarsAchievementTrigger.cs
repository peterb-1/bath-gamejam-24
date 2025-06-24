using Core;
using Core.Saving;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Achievements
{
    public class GetAllDistrictStarsAchievementTrigger : AbstractAchievementTrigger
    {
        [SerializeField] 
        private int district;

        private async void Awake()
        {
            if (SceneLoader.Instance.CurrentSceneConfig.LevelConfig.DistrictNumber != district) return;
            
            await UniTask.WaitUntil(() => SaveManager.IsReady);

            SaveManager.Instance.SaveData.CampaignData.OnNewBestAchieved += HandleNewBestAchieved;
        }

        private void HandleNewBestAchieved()
        {
            var (_, stars, totalMissions) = SaveManager.Instance.SaveData.CampaignData.GetDistrictProgress(district);

            if (stars >= totalMissions * 3)
            {
                TriggerAchievement();
            }
        }

        private void OnDestroy()
        {
            SaveManager.Instance.SaveData.CampaignData.OnNewBestAchieved -= HandleNewBestAchieved;
        }
    }
}