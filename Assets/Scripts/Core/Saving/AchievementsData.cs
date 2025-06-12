using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Achievements;
using Gameplay.Trails;
using UnityEngine;

namespace Core.Saving
{
    [Serializable]
    public class AchievementsData
    {
        [SerializeField] 
        private List<AchievementData> achievements = new();

        public bool IsAchievementWithTrailUnlocked(Trail trail)
        {
            foreach (var achievementData in achievements)
            {
                if (!achievementData.IsUnlocked) continue;
                
                foreach (var achievement in AchievementManager.Instance.Achievements)
                {
                    if (achievementData.Guid == achievement.Guid && achievement.TrailGuid == trail.Guid)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        public bool TryGetAchievementForTrail(Trail trail, out Achievement trailAchievement)
        {
            foreach (var achievementData in achievements)
            {
                foreach (var achievement in AchievementManager.Instance.Achievements)
                {
                    if (achievementData.Guid == achievement.Guid && achievement.TrailGuid == trail.Guid)
                    {
                        trailAchievement = achievement;
                        return true;
                    }
                }
            }

            trailAchievement = null;
            return false;
        }

        public bool TryUnlockAchievement(Achievement unlockedAchievement)
        {
            foreach (var achievement in achievements)
            {
                if (achievement.Guid == unlockedAchievement.Guid)
                {
                    return achievement.TryUnlock();
                }
            }

            return false;
        }
        
        public void MarkAchievementAsPosted(Achievement unlockedAchievement)
        {
            foreach (var achievement in achievements)
            {
                if (achievement.Guid == unlockedAchievement.Guid)
                {
                    achievement.MarkAsPosted();
                }
            }
        }
        
        public bool DoesAchievementNeedPosting(Achievement achievementToCheck)
        {
            foreach (var achievement in achievements)
            {
                if (achievement.Guid == achievementToCheck.Guid)
                {
                    return achievement.IsUnlocked && !achievement.IsPosted;
                }
            }

            return false;
        }

        public async UniTask InitialiseAsync()
        {
            await UniTask.WaitUntil(AchievementManager.IsReady);

            foreach (var achievement in AchievementManager.Instance.Achievements)
            {
                var hasMatchingSaveData = false;
                
                foreach (var achievementData in achievements)
                {
                    if (achievementData.Guid == achievement.Guid)
                    {
                        hasMatchingSaveData = true;
                    }
                }

                if (!hasMatchingSaveData)
                {
                    achievements.Add(new AchievementData(achievement.Guid));
                }
            }
        }

#if UNITY_EDITOR
        public void ResetAllAchievements()
        {
            foreach (var achievementData in achievements)
            {
                achievementData.Reset();
            }
        }
#endif
    }
}