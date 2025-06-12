using System;
using System.Collections.Generic;
using Core.Saving;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using Steam;
using Steamworks;
using UnityEditor;
using UnityEngine;
using Utils;

namespace Gameplay.Achievements
{
    public class AchievementManager : MonoBehaviour
    {
        [SerializeField] 
        private AchievementTriggerData[] achievementTriggers;

        [SerializeField] 
        private bool tryPostOnAwake;

        private List<Achievement> achievements;

        public List<Achievement> Achievements => achievements;

        public static AchievementManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                GameLogger.LogError("Cannot have more than one AchievementManager in the scene at once! Destroying this one.");
                Destroy(this);
                return;
            }

            GatherAchievements();
            
            Instance = this;

            InitialiseSteamAchievementsAsync().Forget();
        }
        
        public static bool IsReady() => Instance != null;

        private void GatherAchievements()
        {
            achievements = new List<Achievement>();

            foreach (var achievementTrigger in achievementTriggers)
            {
                achievements.Add(achievementTrigger.Achievement);
                
                if (achievementTrigger.Trigger == null) continue;
                
                achievementTrigger.Trigger.SetAchievement(achievementTrigger.Achievement);
                achievementTrigger.Trigger.OnAchievementUnlocked += HandleAchievementUnlocked;
            }
        }
        
        private async UniTask InitialiseSteamAchievementsAsync()
        {
            await UniTask.WaitUntil(() => SteamManager.Initialized && SaveManager.IsReady);
            
            SteamUserStats.RequestCurrentStats();

            if (!tryPostOnAwake) return;

            var haveAnyAchievementsBeenPosted = false;
            
            foreach (var achievement in achievements)
            {
                if (SaveManager.Instance.SaveData.AchievementsData.DoesAchievementNeedPosting(achievement))
                {
                    GameLogger.Log($"Identified unposted achievement {achievement.Name} ({achievement.Guid})", this);
                    
                    haveAnyAchievementsBeenPosted |= TryPostAchievementToSteam(achievement);
                }
            }

            if (haveAnyAchievementsBeenPosted)
            {
                SaveManager.Instance.Save();
            }
        }

        private void HandleAchievementUnlocked(Achievement achievement)
        {
            if (SaveManager.Instance.SaveData.AchievementsData.TryUnlockAchievement(achievement))
            {
                GameLogger.Log($"Unlocked achievement {achievement.Name} ({achievement.Guid})", this);
                
                if (TryPostAchievementToSteam(achievement))
                {
                    SaveManager.Instance.Save();
                }
            }
            else
            {
                GameLogger.Log($"Tried to unlock achievement {achievement.Name} ({achievement.Guid}) but failed. It may have already been unlocked!", this);
            }
        }

        private bool TryPostAchievementToSteam(Achievement achievement)
        {
            if (SteamManager.Initialized)
            {
                GameLogger.Log($"Trying to post achievement {achievement.SteamName} ({achievement.Guid}) to Steam...", this);
                
                var hasSetAchievement = SteamUserStats.SetAchievement(achievement.SteamName);
                var hasStoredStats = SteamUserStats.StoreStats();

                if (hasSetAchievement && hasStoredStats)
                {
                    GameLogger.Log($"Posted achievement {achievement.SteamName} ({achievement.Guid}) successfully", this);
                    SaveManager.Instance.SaveData.AchievementsData.MarkAchievementAsPosted(achievement);
                    
                    return true;
                }
                else
                {
                    GameLogger.LogError($"Failed to post achievement {achievement.SteamName} ({achievement.Guid}) to Steam (set {hasSetAchievement}; store {hasStoredStats})", this);
                }
            }
            else
            {
                GameLogger.LogError($"Failed to post achievement {achievement.SteamName} ({achievement.Guid}) to Steam as SteamManager is not initialised!", this);
            }

            return false;
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
                    
                    GameLogger.Log($"Assigned new GUID {newGuid} for achievement {achievement.Name}", this);

                    EditorUtility.SetDirty(achievement);
                }
                else
                {
                    guids.Add(guid);
                }

                checkedAchievements.Add(achievement);
            }
        }
        
        [Button("[DEBUG] Reset Achievements")]
        private void ResetAchievements()
        {
            foreach (var achievement in achievements)
            {
                SteamUserStats.ClearAchievement(achievement.SteamName);
            }
            
            SaveManager.Instance.SaveData.AchievementsData.ResetAllAchievements();
            
            SteamUserStats.StoreStats();
            SaveManager.Instance.Save();
            
            GameLogger.Log("Reset all achievements!", this);
        }
#endif
    }
}