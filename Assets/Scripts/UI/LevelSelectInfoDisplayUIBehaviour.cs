using Core;
using Core.Saving;
using Gameplay.Core;
using TMPro;
using UnityEngine;

namespace UI
{
    public class LevelSelectInfoDisplayUIBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private PageGroup pageGroup;

        [SerializeField] 
        private Page noDataPage;
        
        [SerializeField] 
        private Page levelInfoPage;
        
        [SerializeField] 
        private Page nextDistrictPage;
        
        [SerializeField] 
        private TMP_Text levelCodeText;

        [SerializeField]
        private TMP_Text bestTimeText;

        [SerializeField] 
        private TMP_Text nextDistrictText;

        public void SetNoData()
        {
            pageGroup.SetPage(noDataPage);
        }
        
        public void SetLevelInfo(SceneConfig sceneConfig)
        {
            if (SaveManager.Instance.SaveData.CampaignData.TryGetLevelData(sceneConfig, out var levelData))
            {
                levelCodeText.text = levelData.SceneConfig.LevelConfig.GetLevelNumber();
                bestTimeText.text = TimerBehaviour.GetFormattedTime(levelData.BestTime);
            }
            
            pageGroup.SetPage(levelInfoPage);
        }

        public void SetNextDistrict(int district)
        {
            nextDistrictText.text = $"District {LevelConfig.GetRomanNumeral(district)}";
            
            pageGroup.SetPage(nextDistrictPage);
        }
    }
}