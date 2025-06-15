using Gameplay.Core;
using TMPro;
using UnityEngine;

namespace UI
{
    public class LeaderboardRow : MonoBehaviour
    {
        [SerializeField] 
        private TMP_Text positionText;
        
        [SerializeField] 
        private TMP_Text usernameText;
        
        [SerializeField] 
        private TMP_Text timeText;
        
        [SerializeField] 
        private RankingStarUIBehaviour rankingStarUIBehaviour;

        public void SetDetails(int position, string username, float time, LevelConfig levelConfig)
        {
            positionText.text = $"{position}";
            usernameText.text = username;
            timeText.text = TimerBehaviour.GetFormattedTime(time);
            rankingStarUIBehaviour.SetRanking(levelConfig.GetTimeRanking(time));
        }
    }
}