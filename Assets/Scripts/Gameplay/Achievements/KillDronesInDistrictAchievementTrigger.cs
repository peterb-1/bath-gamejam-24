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

        private void Awake()
        {
            if (SceneLoader.Instance.CurrentSceneConfig.LevelConfig.DistrictNumber != validDistrict) return;
            
            DroneTrackerService.OnDroneKilled += HandleDroneKilled;
        }

        private void HandleDroneKilled(DroneHitboxBehaviour drone)
        {
            var sceneLoader = SceneLoader.Instance;

            if (sceneLoader.CurrentLevelData.TryNotifyDroneKilled(drone.Id))
            {
                var districtDronesKilled = 0;
                
                foreach (var sceneConfig in sceneLoader.SceneConfigs)
                {
                    if (sceneConfig.IsLevelScene &&
                        sceneConfig.LevelConfig.DistrictNumber == validDistrict &&
                        SaveManager.Instance.SaveData.CampaignData.TryGetLevelData(sceneConfig.LevelConfig, out var levelData))
                    {
                        districtDronesKilled += levelData.DronesKilled.Count;
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