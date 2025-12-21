using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils;

namespace Gameplay.Events
{
    public abstract class AbstractEventTrigger : MonoBehaviour
    {
        [SerializeField] 
        private AbstractEventAction[] actions;

        private bool hasTriggered;

        protected void TriggerSequence()
        {
            if (hasTriggered) return;
            
            hasTriggered = true;
                
            RunSequenceAsync().Forget();
        }

        private async UniTask RunSequenceAsync()
        {
            GameLogger.Log($"Running event trigger {name}...", this);
            
            foreach (var action in actions)
            {
                if (!action.IsActive) continue;
                
                if (action.ShouldAwaitExecution)
                {
                    await action.Execute();
                }
                else
                {
                    action.Execute();
                }
            }
            
            GameLogger.Log($"Completed execution of event trigger {name}!", this);
        }
    }
}