using Gameplay.Core;
using UnityEngine;

namespace UI
{
    public class RankingStarUIBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private RankingStar firstStar;
        
        [SerializeField] 
        private RankingStar secondStar;
        
        [SerializeField] 
        private RankingStar thirdStar;

        public void SetRanking(TimeRanking ranking)
        {
            var isRainbow = ranking == TimeRanking.Rainbow;
                
            firstStar.SetActive(ranking >= TimeRanking.OneStar, shouldAnimate: false);
            secondStar.SetActive(ranking >= TimeRanking.TwoStar, shouldAnimate: false);
            thirdStar.SetActive(ranking >= TimeRanking.ThreeStar, shouldAnimate: false);
                
            firstStar.SetRainbowState(isRainbow, shouldAnimate: false);
            secondStar.SetRainbowState(isRainbow, shouldAnimate: false);
            thirdStar.SetRainbowState(isRainbow, shouldAnimate: false);
        }
    }
}