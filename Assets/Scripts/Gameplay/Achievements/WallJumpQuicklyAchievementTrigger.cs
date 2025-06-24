using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Player;
using UnityEngine;

namespace Gameplay.Achievements
{
    public class WallJumpQuicklyAchievementTrigger : AbstractAchievementTrigger
    {
        [SerializeField] 
        private int countRequired;
        
        [SerializeField] 
        private int timeWindow;

        private PlayerMovementBehaviour playerMovementBehaviour;

        private readonly Queue<float> wallJumpTimestamps = new();

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerMovementBehaviour = PlayerAccessService.Instance.PlayerMovementBehaviour;
            
            playerMovementBehaviour.OnWallJump += HandleWallJump;
        }

        private void HandleWallJump()
        {
            var currentTime = Time.unscaledTime;
            
            if (wallJumpTimestamps.Count >= countRequired - 1)
            {
                var oldestTime = wallJumpTimestamps.Dequeue();

                if (currentTime - timeWindow <= oldestTime)
                {
                    TriggerAchievement();
                    
                    playerMovementBehaviour.OnWallJump -= HandleWallJump;

                    return;
                }
            }
            
            wallJumpTimestamps.Enqueue(currentTime);
        }

        private void OnDestroy()
        {
            if (playerMovementBehaviour != null)
            {
                playerMovementBehaviour.OnWallJump -= HandleWallJump;
            }
        }
    }
}