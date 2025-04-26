using System;
using System.Collections.Generic;
using Core.Saving;
using UnityEditor;
using UnityEngine;
using Utils;

namespace Gameplay.Achievements
{
    public class AchievementManager : MonoBehaviour
    {
        [SerializeField] 
        private AchievementTriggerData[] achievementTriggers;

        private List<Achievement> achievements;

        public List<Achievement> Achievements => achievements;

        public static AchievementManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                GameLogger.LogError("Cannot have more than one CameraAccessService in the scene at once! Destroying this one.");
                Destroy(this);
                return;
            }

            Instance = this;

            achievements = new List<Achievement>();

            foreach (var achievementTrigger in achievementTriggers)
            {
                achievements.Add(achievementTrigger.Achievement);
                
                if (achievementTrigger.Trigger == null) continue;
                
                achievementTrigger.Trigger.SetAchievement(achievementTrigger.Achievement);
                achievementTrigger.Trigger.OnAchievementUnlocked += HandleAchievementUnlocked;
            }
        }
        
        public static bool IsReady() => Instance != null;

        private void HandleAchievementUnlocked(Achievement achievement)
        {
            if (SaveManager.Instance.SaveData.AchievementsData.TryUnlockAchievement(achievement))
            {
                GameLogger.Log($"Unlocked achievement {achievement.Guid}!", this);
                
                SaveManager.Instance.Save();
            }
            else
            {
                GameLogger.Log($"Tried to unlock achievement {achievement.Guid} but failed.", this);
            }
        }

        private void OnDestroy()
        {
            foreach (var achievementTrigger in achievementTriggers)
            {
                if (achievementTrigger.Trigger == null) continue;
                
                achievementTrigger.Trigger.OnAchievementUnlocked -= HandleAchievementUnlocked;
            }
            
            Instance = null;
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            var guids = new HashSet<string>();
            var checkedAchievements = new HashSet<Achievement>();
            
            foreach (var achievementTrigger in achievementTriggers)
            {
                var achievement = achievementTrigger.Achievement;
                
                if (checkedAchievements.Contains(achievement)) continue;
                
                var guid = achievement.Guid;

                if (string.IsNullOrWhiteSpace(guid) || guids.Contains(guid))
                {
                    var newGuid = Guid.NewGuid();

                    achievement.SetGuid(Guid.NewGuid());
                    guids.Add(newGuid.ToString());
                    
                    EditorUtility.SetDirty(achievement);
                }
                else
                {
                    guids.Add(guid);
                }

                checkedAchievements.Add(achievement);
            }
        }
#endif
    }
}