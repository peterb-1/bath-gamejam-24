using Core.Saving;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;

namespace Gameplay.Achievements
{
    public class StatsValueAchievementTrigger : AbstractAchievementTrigger
    {
        [SerializeField]
        private StatType statType;

        [SerializeField] 
        private bool isInteger;

        [SerializeField, ShowIf(nameof(isInteger))]
        private int intValue;
        
        [SerializeField, HideIf(nameof(isInteger))]
        private float floatValue;
        
        private async void Awake()
        {
            await UniTask.WaitUntil(() => SaveManager.IsReady);

            if (isInteger)
            {
                SaveManager.Instance.SaveData.StatsData.RegisterStatInterest(statType, intValue, TriggerAchievement);
            }
            else
            {
                SaveManager.Instance.SaveData.StatsData.RegisterStatInterest(statType, floatValue, TriggerAchievement);
            }
        }
    }
}