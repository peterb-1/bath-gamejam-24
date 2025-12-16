using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Drone;
using UnityEngine;

namespace Gameplay.Events
{
    public class DroneActivateAction : AbstractEventAction
    {
        [SerializeField]
        private List<DroneFlyInBehaviour> flyInDrones;
        
        [SerializeField]
        private List<DronePatrolBehaviour> patrolDrones;
        
        public override UniTask Execute()
        {
            foreach (var drone in flyInDrones)
            {
                drone.Activate();
            }

            foreach (var drone in patrolDrones)
            {
                drone.ActivateAsync().Forget();
            }
            
            return UniTask.CompletedTask;
        }
    }
}