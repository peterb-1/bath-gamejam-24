﻿using System;
using System.Collections.Generic;
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
        public StatsData StatsData { get; private set; }
        
        [field: SerializeField]
        public PreferenceData PreferenceData { get; private set; }

        public async UniTask InitialiseAsync(AbstractSettingBase[] settings)
        {
            CampaignData ??= new CampaignData();
            AchievementsData ??= new AchievementsData();
            StatsData ??= new StatsData();
            PreferenceData ??= new PreferenceData();

            await CampaignData.InitialiseAsync();
            await AchievementsData.InitialiseAsync();
            
            PreferenceData.Initialise(settings);
            StatsData.Initialise();
        }
    }
}