using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Events
{
    public class ActivateActionAction : AbstractEventAction
    {
        [SerializeField] 
        private AbstractEventAction action;
        
        [SerializeField] 
        private bool shouldActivate;
        
        public override UniTask Execute()
        {
            action.SetActive(shouldActivate);
            
            return UniTask.CompletedTask;
        }
    }
}
