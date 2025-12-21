using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Events
{
    public class ActivateTriggerAction : AbstractEventAction
    {
        [SerializeField] 
        private AbstractEventTrigger trigger;
        
        [SerializeField] 
        private bool shouldActivate;
        
        public override UniTask Execute()
        {
            trigger.SetActive(shouldActivate);
            
            return UniTask.CompletedTask;
        }
    }
}
