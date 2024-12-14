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

        public async UniTask InitialiseAsync()
        {
            CampaignData ??= new CampaignData();

            await CampaignData.InitialiseAsync();
        }
    }
}