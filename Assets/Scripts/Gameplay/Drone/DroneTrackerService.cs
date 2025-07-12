using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Drone
{
    public class DroneTrackerService : MonoBehaviour
    {
        private static readonly HashSet<DroneHitboxBehaviour> ActiveDrones = new();
        
        public static event Action<DroneHitboxBehaviour> OnDroneKilled;
        public static event Action<DroneHitboxBehaviour> OnDroneKilledByGhost;

        public static void RegisterDrone(DroneHitboxBehaviour drone)
        {
            ActiveDrones.Add(drone);
            drone.OnDroneKilled += HandleDroneKilled;
        }
        
        private static void HandleDroneKilled(DroneHitboxBehaviour drone)
        {
            ActiveDrones.Remove(drone);
            drone.OnDroneKilled -= HandleDroneKilled;
                
            OnDroneKilled?.Invoke(drone);
        }

        public static void KillDroneFromSpectatorGhost(ushort droneId, Vector2 ghostPosition)
        {
            DroneHitboxBehaviour killedDrone = null;
            
            foreach (var drone in ActiveDrones)
            {
                if (drone.Id == droneId)
                {
                    killedDrone = drone;
                    
                    drone.OnDroneKilled -= HandleDroneKilled;
                    drone.NotifyKilledByGhost(ghostPosition);
                    
                    OnDroneKilledByGhost?.Invoke(drone);
                }
            }

            if (killedDrone != null)
            {
                ActiveDrones.Remove(killedDrone);
            }
        }

        private void OnDestroy()
        {
            foreach (var drone in ActiveDrones)
            {
                drone.OnDroneKilled -= HandleDroneKilled;
            }
            
            ActiveDrones.Clear();
        }
    }
}