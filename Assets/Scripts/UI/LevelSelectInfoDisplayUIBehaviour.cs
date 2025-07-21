using Core;
using Core.Saving;
using Cysharp.Threading.Tasks;
using Gameplay.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        
        [SerializeField] 
        private TMP_Text noCollectibleText;
        
        [SerializeField] 
        private Image collectibleImage;
        
        [SerializeField] 
        private CollectibleUIBehaviour collectibleUIBehaviour;

        [SerializeField] 
        private RankingStarUIBehaviour rankingStarUIBehaviour;

        public void SetNoData()
        {
            pageGroup.SetPage(noDataPage);
        }
        
        public async UniTask SetLevelInfoAsync(LevelConfig levelConfig)
        {
            // weird quirk where the await creates some delay even when the condition is already true
            if (!SaveManager.IsReady)
            {
                await UniTask.WaitUntil(() => SaveManager.IsReady);
            }

            levelCodeText.text = levelConfig.GetLevelCode();
            
            if (SaveManager.Instance.SaveData.CampaignData.TryGetLevelData(levelConfig, out var levelData))
            {
                bestTimeText.text = TimerBehaviour.GetFormattedTime(levelData.BestMilliseconds);

                if (levelConfig.HasCollectible)
                {
                    collectibleUIBehaviour.SetCollected(levelData.HasFoundCollectible);
                }
                
                rankingStarUIBehaviour.SetRanking(levelConfig.GetTimeRanking(levelData.BestMilliseconds));
            }
            
            collectibleImage.enabled = levelConfig.HasCollectible;
            noCollectibleText.enabled = !levelConfig.HasCollectible;

            pageGroup.SetPage(levelInfoPage);
        }

        public void SetNextDistrict(int district)
        {
            nextDistrictText.text = LevelConfig.GetDistrictName(district);
            
            pageGroup.SetPage(nextDistrictPage);
        }
    }
}