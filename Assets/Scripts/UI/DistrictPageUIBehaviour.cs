using Core.Saving;
using Gameplay.Core;
using TMPro;
using UnityEngine;

namespace UI
{
    public class DistrictPageUIBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private int districtNumber;
        
        [SerializeField] 
        private TMP_Text districtNameText;
        
        [field: SerializeField]
        public Page Page { get; private set; }

        [field: SerializeField]
        public LevelSelectButton[] LevelSelectButtons { get; private set; }

        private void Awake()
        {
            districtNameText.text = $"{LevelConfig.GetRomanNumeral(districtNumber)}. {LevelConfig.GetDistrictName(districtNumber)}";
        }

        public LevelSelectButton GetLeftmostUnlockedLevelButton()
        {
            foreach (var button in LevelSelectButtons)
            {
                if (SaveManager.Instance.SaveData.CampaignData.TryGetLevelData(button.SceneConfig.LevelConfig, out var levelData) &&
                    levelData.IsUnlocked)
                {
                    return button;
                }
            }
            
            return null;
        }
        
        public LevelSelectButton GetRightmostUnlockedLevelButton()
        {
            for (var i = LevelSelectButtons.Length - 1; i >= 0; i--)
            {
                var button = LevelSelectButtons[i];
                
                if (SaveManager.Instance.SaveData.CampaignData.TryGetLevelData(button.SceneConfig.LevelConfig, out var levelData) &&
                    levelData.IsUnlocked)
                {
                    return button;
                }
            }

            return null;
        }
    }
}