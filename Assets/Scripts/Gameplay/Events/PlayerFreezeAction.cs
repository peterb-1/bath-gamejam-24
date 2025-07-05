using System;
using Cysharp.Threading.Tasks;
using Gameplay.Input;
using UnityEngine;

namespace Gameplay.Events
{
    public class PlayerFreezeAction : AbstractEventAction
    {
        [SerializeField]
        private float freezeDuration;
        
        public async override UniTask Execute()
        {
            InputManager.Instance.DisableGameplayInputs();
            await UniTask.Delay(TimeSpan.FromSeconds(freezeDuration));
            InputManager.Instance.EnableGameplayInputs();
        }
    }
}