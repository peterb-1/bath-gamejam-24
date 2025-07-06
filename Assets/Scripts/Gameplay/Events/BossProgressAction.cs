using System;
using Cysharp.Threading.Tasks;
using Gameplay.Boss;
using UnityEngine;

namespace Gameplay.Events
{
    public class BossProgressAction : AbstractEventAction
    {
        [SerializeField]
        BossMovementBehaviour bossMovementBehaviour;
        
        public override async UniTask Execute()
        {
            bossMovementBehaviour.QueueIncrementProgress();
        }
    }
}