using Gameplay.Boss;
using UnityEngine;

namespace Gameplay.Events
{
    public class BossProgressTrigger : AbstractEventTrigger
    {
        [SerializeField]
        private BossMovementBehaviour bossMovementBehaviour;

        [SerializeField]
        private int progressToTrigger;
        
        public override void Awake()
        {
            base.Awake();
            
            bossMovementBehaviour.OnBossProgress += TryTriggerEvent;
        }

        private void TryTriggerEvent(BossMovementBehaviour boss)
        {
            if (boss.GetNextPointIndex() == progressToTrigger)
            {
                TriggerSequence();
            }
        }
    }
}