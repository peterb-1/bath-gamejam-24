using Core;
using Core.Saving;
using Gameplay.Drone;
using UnityEngine;

namespace Gameplay.Achievements
{
    public class KillDronesInDistrictAchievementTrigger : AbstractAchievementTrigger
    {
        [SerializeField] 
        private int validDistrict;
        
        [SerializeField] 
        private int dronesRequired;

        private int dronesKilled;

        private void Awake()
        {
            if (SceneLoader.Instance.CurrentSceneConfig.LevelConfig.DistrictNumber != validDistrict) return;
            
            DroneTrackerService.OnDroneKilled += HandleDroneKilled;
        }

        private void HandleDroneKilled(DroneHitboxBehaviour _)
        {
            dronesKilled++;
            
            var sceneLoader = SceneLoader.Instance;
            var campaignData = SaveManager.Instance.SaveData.CampaignData;

            if (campaignData.TryGetLevelData(sceneLoader.CurrentSceneConfig.LevelConfig, out var currentLevelData) && 
                currentLevelData.TrySetDronesKilled(dronesKilled))
            {
                var districtDronesKilled = 0;
                
                foreach (var sceneConfig in sceneLoader.SceneConfigs)
                {
                    if (sceneConfig.IsLevelScene &&
                        sceneConfig.LevelConfig.DistrictNumber == validDistrict &&
                        campaignData.TryGetLevelData(sceneConfig.LevelConfig, out var levelData))
                    {
                        districtDronesKilled += levelData.DronesKilled;
                    }
                }

                if (districtDronesKilled >= dronesRequired)
                {
                    TriggerAchievement();
                    
                    DroneTrackerService.OnDroneKilled -= HandleDroneKilled;
                }
            }
        }

        private void OnDestroy()
        {
            DroneTrackerService.OnDroneKilled -= HandleDroneKilled;
        }
    }
}