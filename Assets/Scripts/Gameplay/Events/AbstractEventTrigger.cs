using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils;

namespace Gameplay.Events
{
    public abstract class AbstractEventTrigger : MonoBehaviour
    {
        [SerializeField] 
        private AbstractEventAction[] actions;

        [SerializeField] 
        private bool isActiveOnAwake = true;
        
        private bool hasTriggered;
        
        public bool IsActive { get; private set; }

        public virtual void Awake()
        {
            IsActive = isActiveOnAwake;
        }
        
        protected void TriggerSequence()
        {
            if (hasTriggered || !IsActive) return;
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
        
        public void SetActive(bool isActive)
        {
            IsActive = isActive;
        }
    }
}