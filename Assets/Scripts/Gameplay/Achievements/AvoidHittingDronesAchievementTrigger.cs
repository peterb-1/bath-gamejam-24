using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Drone;
using Gameplay.Player;
using UnityEngine;

namespace Gameplay.Achievements
{
    public class AvoidHittingDronesAchievementTrigger : AbstractAchievementTrigger
    {
        [SerializeField] 
        private int validDistrict;
        
        private PlayerVictoryBehaviour playerVictoryBehaviour;

        private async void Awake()
        {
            if (SceneLoader.Instance.CurrentSceneConfig.LevelConfig.DistrictNumber != validDistrict) return;

            await UniTask.WaitUntil(PlayerAccessService.IsReady);
            
            playerVictoryBehaviour = PlayerAccessService.Instance.PlayerVictoryBehaviour;
            playerVictoryBehaviour.OnVictorySequenceFinish += HandleVictorySequenceFinish;
            
            DroneTrackerService.OnDroneKilled += HandleDroneKilled;
        }

        private void HandleVictorySequenceFinish(float _)
        {
            TriggerAchievement();
        }

        private void HandleDroneKilled(DroneHitboxBehaviour _)
        {
            playerVictoryBehaviour.OnVictorySequenceFinish -= HandleVictorySequenceFinish;
            
            DroneTrackerService.OnDroneKilled -= HandleDroneKilled;
        }

        private void OnDestroy()
        {
            if (playerVictoryBehaviour != null)
            {
                playerVictoryBehaviour.OnVictorySequenceFinish -= HandleVictorySequenceFinish;
            }

            DroneTrackerService.OnDroneKilled -= HandleDroneKilled;
        }
    }
}