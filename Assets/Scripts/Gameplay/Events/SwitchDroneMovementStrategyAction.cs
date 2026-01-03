using Cysharp.Threading.Tasks;
using Gameplay.Drone;
using UnityEngine;
using Utils;

namespace Gameplay.Events
{
    public class SwitchDroneMovementStrategyAction : AbstractEventAction
    {
        [SerializeField] 
        private DroneMovementBehaviour drone;

        [SerializeReference, SubclassSelector] 
        private IDroneMovementStrategy newStrategy;
        
        public override UniTask Execute()
        {
            drone.SetMovementStrategy(newStrategy);
            
            return UniTask.CompletedTask;
        }
    }
}