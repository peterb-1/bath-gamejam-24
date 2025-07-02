using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Drone;
using Gameplay.Player;
using UnityEngine;

namespace Gameplay.Events
{
    public class PlayerFreezeAction : AbstractEventAction
    {
        [SerializeField]
        private float freezeDuration;
        
        private PlayerMovementBehaviour playerMovementBehaviour;

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);
            playerMovementBehaviour = PlayerAccessService.Instance.PlayerMovementBehaviour;
        }

        public async override UniTask Execute()
        {
            playerMovementBehaviour.SetMovementEnabled(false);
            await UniTask.Delay(TimeSpan.FromSeconds(freezeDuration));
            playerMovementBehaviour.SetMovementEnabled(true);
        }
    }
}