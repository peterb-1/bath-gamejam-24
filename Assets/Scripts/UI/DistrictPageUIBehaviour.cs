using Core.Saving;
using UnityEngine;

namespace UI
{
    public class DistrictPageUIBehaviour : MonoBehaviour
    {
        [field: SerializeField]
        public Page Page { get; private set; }

        [field: SerializeField]
        public LevelSelectButton[] LevelSelectButtons { get; private set; }

        public LevelSelectButton GetLeftmostUnlockedLevelButton()
        {
            foreach (var button in LevelSelectButtons)
            {
                if (SaveManager.Instance.SaveData.CampaignData.TryGetLevelData(button.SceneConfig, out var levelData) &&
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
                
                if (SaveManager.Instance.SaveData.CampaignData.TryGetLevelData(button.SceneConfig, out var levelData) &&
                    levelData.IsUnlocked)
                {
                    return button;
                }
            }

            return null;
        }
    }
}