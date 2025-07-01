using Core.Saving;
using Gameplay.Trails;
using TMPro;
using UnityEngine;
using Utils;

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
        private Page unlockedByDefaultPage;

        [SerializeField] 
        private Page lockedPage;

        [SerializeField] 
        private TMP_Text unlockedTrailNameText;
        
        [SerializeField] 
        private TMP_Text unlockedByDefaultTrailNameText;
        
        [SerializeField] 
        private TMP_Text lockedAchievementNameText;
        
        [SerializeField] 
        private TMP_Text unlockedAchievementNameText;
        
        [SerializeField] 
        private TMP_Text lockedAchievementDescriptionText;
        
        [SerializeField] 
        private TMP_Text unlockedAchievementDescriptionText;
        
        public void SetNoData()
        {
            pageGroup.SetPage(noDataPage);
        }

        public void SetTrailInfo(Trail trail)
        {
            var isUnlocked = trail.IsUnlockedByDefault ||
                             SaveManager.Instance.SaveData.AchievementsData.IsAchievementWithTrailUnlocked(trail);

            if (isUnlocked)
            {
                var nameText = trail.IsUnlockedByDefault ? unlockedByDefaultTrailNameText : unlockedTrailNameText;
                nameText.text = trail.Name;
            }

            var hasAchievement = SaveManager.Instance.SaveData.AchievementsData.TryGetAchievementForTrail(trail, out var achievement);

            if (hasAchievement)
            {
                var page = isUnlocked ? unlockedPage : lockedPage;
                var achievementNameText = isUnlocked ? unlockedAchievementNameText : lockedAchievementNameText;
                var achievementDescriptionText = isUnlocked ? unlockedAchievementDescriptionText : lockedAchievementDescriptionText;

                achievementNameText.text = achievement.Name;
                achievementDescriptionText.text = achievement.UnlockDescription;

                pageGroup.SetPage(page);
            }
            else if (trail.IsUnlockedByDefault)
            {
                pageGroup.SetPage(unlockedByDefaultPage);
            }
            else
            {
                GameLogger.LogWarning("Trying to set trail info for a locked trail with no associated achievement!", this);
            }
        }
    }
}