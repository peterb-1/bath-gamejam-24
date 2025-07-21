using System;
using Gameplay.Core;
using Gameplay.Drone;
using UnityEngine;
using Utils;

namespace UI
{
    public class TimerBehaviour : MonoBehaviour
    {
        public float TimeElapsed { get; private set; }
        public float RealtimeElapsed { get; private set; }

        private float realtimeOnStart;

        public event Action<float> OnTimeBonusApplied;

        private void Awake()
        {
            DroneTrackerService.OnDroneKilled += HandleDroneKilled;
            DroneTrackerService.OnDroneKilledByGhost += HandleDroneKilled;
        }

        private void Start()
        {
            TimeElapsed = 0f;
            RealtimeElapsed = 0f;
            realtimeOnStart = TimeManager.Instance.UnpausedRealtimeSinceStartup;
        }
        
        private void Update()
        {
            TimeElapsed += Time.deltaTime;
            RealtimeElapsed = TimeManager.Instance.UnpausedRealtimeSinceStartup - realtimeOnStart;
        }

        private void HandleDroneKilled(DroneHitboxBehaviour drone)
        {
            TimeElapsed -= drone.TimeBonusOnKilled;

            if (drone.TimeBonusOnKilled > 0f)
            {
                OnTimeBonusApplied?.Invoke(drone.TimeBonusOnKilled);
            }
        }

        public string GetFormattedTimeElapsed()
        {
            return GetFormattedTime(TimeElapsed.ToMilliseconds());
        }

        public static string GetFormattedTime(int milliseconds)
        {
            if (milliseconds >= int.MaxValue - 9) return "N/A";

            // so that e.g. 12.201, which fails a 12.000 threshold, is displayed as 12.01 rather than 12.00
            milliseconds += 9;

            var minutes = milliseconds / 60000;
            var seconds = (milliseconds % 60000) / 1000;
            var centiSeconds = (milliseconds % 1000) / 10;

            return $"{minutes:00}:{seconds:00}:{centiSeconds:00}";
        }

        private void OnDestroy()
        {
            DroneTrackerService.OnDroneKilled -= HandleDroneKilled;
            DroneTrackerService.OnDroneKilledByGhost -= HandleDroneKilled;
        }
    }
}