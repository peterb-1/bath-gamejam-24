using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Environment;
using UnityEngine;

namespace Gameplay.Events
{
    public class LaserToggleAction : AbstractEventAction
    {
        [SerializeField]
        private List<Laser> lasers;

        [SerializeField]
        private float duration;
        
        public override UniTask Execute()
        {
            foreach (var laser in lasers)
            {
                laser.ToggleSuppression(duration);
            }
            
            return UniTask.CompletedTask;
        }
    }
}