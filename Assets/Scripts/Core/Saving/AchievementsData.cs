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
    }
}