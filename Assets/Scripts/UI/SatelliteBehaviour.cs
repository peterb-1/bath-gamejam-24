using Core.Saving;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SatelliteBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private Image image;

        [SerializeField]
        private Sprite[] panelSprites;
        
        [SerializeField] 
        private DistrictPageUIBehaviour districtPageUIBehaviour;

        private void Awake()
        {
            var panelCount = 0;
            
            foreach (var button in districtPageUIBehaviour.LevelSelectButtons)
            {
                var levelConfig = button.SceneConfig.LevelConfig;
                
                if (levelConfig.HasCollectible && 
                    SaveManager.Instance.SaveData.CampaignData.TryGetLevelData(levelConfig, out var levelData) &&
                    levelData.HasFoundCollectible)
                {
                    panelCount++;
                }

                if (levelConfig.IsHidden)
                {
                    if (SaveManager.Instance.SaveData.CampaignData.TryGetLevelData(levelConfig, out var hiddenLevelData) && hiddenLevelData.IsUnlocked)
                    {
                        gameObject.SetActive(false);
                        return;
                    }
                    
                    transform.position = button.transform.position;
                }
            }
            
            if (panelCount < panelSprites.Length)
            {
                image.sprite = panelSprites[panelCount];
            }
            else
            {
                // in theory we'll never get here, since the hidden level should be unlocked by this point, but just for safety
                gameObject.SetActive(false);
            }
        }
    }
}