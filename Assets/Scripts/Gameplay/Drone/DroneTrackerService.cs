using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Drone
{
    public class DroneTrackerService : MonoBehaviour
    {
        private static readonly HashSet<DroneHitboxBehaviour> ActiveDrones = new();
        
        public static event Action<DroneHitboxBehaviour> OnDroneKilled;

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