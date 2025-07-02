using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Events
{
    public class DelayAction : AbstractEventAction
    {
        [SerializeField] 
        private float delayDuration;

        public override async UniTask Execute()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delayDuration));
        }
    }
}