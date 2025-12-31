using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Drone;
using UnityEngine;

namespace Gameplay.Events
{
    public class DroneActivateAction : AbstractEventAction
    {
        [SerializeField]
        private List<DroneMovementBehaviour> patrolDrones;
        
        public override UniTask Execute()
        {
            foreach (var drone in patrolDrones)
            {
                drone.Activate();
            }
            
            return UniTask.CompletedTask;
        }
    }
}