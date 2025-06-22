using Core.Saving;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Achievements
{
    public class GetRainbowRanksAchievementTrigger : AbstractAchievementTrigger
    {
        [SerializeField]
        private int rainbowRanksRequired;
        
        private async void Awake()
        {
            await UniTask.WaitUntil(() => SaveManager.IsReady);

            SaveManager.Instance.SaveData.CampaignData.OnRainbowRanksAchieved += HandleRainbowRanksAchieved;
        }

        private void HandleRainbowRanksAchieved(int count)
        {
            if (count >= rainbowRanksRequired)
            {
                TriggerAchievement();
            }
        }

        private void OnDestroy()
        {
            SaveManager.Instance.SaveData.CampaignData.OnRainbowRanksAchieved -= HandleRainbowRanksAchieved;
        }
    }
}