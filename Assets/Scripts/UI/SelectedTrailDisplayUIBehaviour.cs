using Core.Saving;
using Gameplay.Trails;
using TMPro;
using UnityEngine;

namespace UI
{
    public class SelectedTrailDisplayUIBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private PageGroup pageGroup;

        [SerializeField] 
        private Page noDataPage;
        
        [SerializeField] 
        private Page unlockedPage;

        [SerializeField] 
        private Page lockedPage;

        [SerializeField] 
        private TMP_Text trailNameText;
        
        [SerializeField] 
        private TMP_Text achievementNameText;
        
        public void SetNoData()
        {
            pageGroup.SetPage(noDataPage);
        }

        public void SetTrailInfo(Trail trail)
        {
            trailNameText.text = trail.Name;
            
            var isUnlocked = trail.IsUnlockedByDefault ||
                             SaveManager.Instance.SaveData.AchievementsData.IsAchievementWithTrailUnlocked(trail);

            var page = isUnlocked ? unlockedPage : lockedPage;
            
            pageGroup.SetPage(page);
        }
    }
}