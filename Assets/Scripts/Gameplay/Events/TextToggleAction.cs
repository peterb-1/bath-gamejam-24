using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UI;
using UnityEngine;

namespace Gameplay.Events
{
    public class TextToggleAction : AbstractEventAction
    {
        [SerializeField]
        private List<WorldSpaceText> texts;
        
        public override UniTask Execute()
        {
            foreach (var text in texts)
            {
                text.ToggleVisibility();
            }
            
            return UniTask.CompletedTask;
        }
    }
}