using Core.Saving;
using Cysharp.Threading.Tasks;
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
        
        [SerializeField] 
        private TMP_Text totalCompletedText;
        
        [SerializeField] 
        private TMP_Text totalStarsText;
        
        [field: SerializeField]
        public Page Page { get; private set; }

        [field: SerializeField]
        public LevelSelectButton[] LevelSelectButtons { get; private set; }

        private void Awake()
        {
            SetInfoAsync().Forget();
        }
        
        private async UniTask SetInfoAsync()
        {
            await UniTask.WaitUntil(() => SaveManager.IsReady);
            
            var totalCompleted = 0;
            var totalStars = 0;
            
            foreach (var button in LevelSelectButtons)
            {
                var levelConfig = button.SceneConfig.LevelConfig;

                if (SaveManager.Instance.SaveData.CampaignData.TryGetLevelData(levelConfig, out var levelData))
                {
                    totalStars += levelConfig.GetStars(levelData.BestTime);

                    if (levelData.IsComplete())
                    {
                        totalCompleted++;
                    }
                }
            }

            districtNameText.text = $"{LevelConfig.GetRomanNumeral(districtNumber)}. {LevelConfig.GetDistrictName(districtNumber)}";
            totalCompletedText.text = $"{totalCompleted} / {LevelSelectButtons.Length}";
            totalStarsText.text = $"{totalStars} / {3 * LevelSelectButtons.Length}";
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