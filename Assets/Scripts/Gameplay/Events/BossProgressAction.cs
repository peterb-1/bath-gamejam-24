using Cysharp.Threading.Tasks;
using Gameplay.Boss;
using UnityEngine;

namespace Gameplay.Events
{
    public class BossProgressAction : AbstractEventAction
    {
        [SerializeField]
        private BossMovementBehaviour bossMovementBehaviour;
        
        public override UniTask Execute()
        {
            bossMovementBehaviour.QueueIncrementProgress();
            
            return UniTask.CompletedTask;
        }
    }
}