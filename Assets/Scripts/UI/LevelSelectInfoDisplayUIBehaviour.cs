using Core;
using Core.Saving;
using Cysharp.Threading.Tasks;
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
        
        [SerializeField] 
        private RankingStar firstStar;
        
        [SerializeField] 
        private RankingStar secondStar;
        
        [SerializeField] 
        private RankingStar thirdStar;

        public void SetNoData()
        {
            pageGroup.SetPage(noDataPage);
        }
        
        public async UniTask SetLevelInfoAsync(SceneConfig sceneConfig)
        {
            // weird quirk where the await creates some delay even when the condition is already true
            if (!SaveManager.IsReady)
            {
                await UniTask.WaitUntil(() => SaveManager.IsReady);
            }

            if (sceneConfig.IsLevelScene)
            {
                levelCodeText.text = sceneConfig.LevelConfig.GetLevelCode();
                
                if (SaveManager.Instance.SaveData.CampaignData.TryGetLevelData(sceneConfig.LevelConfig, out var levelData))
                {
                    bestTimeText.text = TimerBehaviour.GetFormattedTime(levelData.BestTime);
                }
                
                var ranking = sceneConfig.LevelConfig.GetTimeRanking(levelData.BestTime);
                var isRainbow = ranking == TimeRanking.Rainbow;
                
                firstStar.SetActive(ranking >= TimeRanking.OneStar, shouldAnimate: false);
                secondStar.SetActive(ranking >= TimeRanking.TwoStar, shouldAnimate: false);
                thirdStar.SetActive(ranking >= TimeRanking.ThreeStar, shouldAnimate: false);
                
                firstStar.SetRainbowState(isRainbow, shouldAnimate: false);
                secondStar.SetRainbowState(isRainbow, shouldAnimate: false);
                thirdStar.SetRainbowState(isRainbow, shouldAnimate: false);
            }
            
            pageGroup.SetPage(levelInfoPage);
        }

        public void SetNextDistrict(int district)
        {
            nextDistrictText.text = LevelConfig.GetDistrictName(district);
            
            pageGroup.SetPage(nextDistrictPage);
        }
    }
}