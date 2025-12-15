using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Environment;
using UnityEngine;

namespace Gameplay.Events
{
    public class LaserActivateAction : AbstractEventAction
    {
        [SerializeField]
        private List<Laser> lasers;

        [SerializeField]
        private float duration;
        
        public override async UniTask Execute()
        {
            foreach (var laser in lasers)
            {
                await laser.ActivateLaserAsync(duration);
            }
        }
    }
}