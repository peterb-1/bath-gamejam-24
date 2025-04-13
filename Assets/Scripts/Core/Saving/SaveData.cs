using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Saving
{
    [Serializable]
    public class SaveData
    {
        [field: SerializeField]
        public CampaignData CampaignData { get; private set; }
        
        [field: SerializeField]
        public AchievementsData AchievementsData { get; private set; }
        
        [field: SerializeField]
        public PreferenceData PreferenceData { get; private set; }

        public async UniTask InitialiseAsync()
        {
            CampaignData ??= new CampaignData();
            AchievementsData ??= new AchievementsData();
            PreferenceData ??= new PreferenceData();

            await CampaignData.InitialiseAsync();
            await AchievementsData.InitialiseAsync();
            await PreferenceData.InitialiseAsync();
        }
    }
}