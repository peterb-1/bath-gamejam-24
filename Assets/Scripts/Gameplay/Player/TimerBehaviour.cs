using System;
using Gameplay.Core;
using Gameplay.Drone;
using UnityEngine;

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
            return GetFormattedTime(TimeElapsed);
        }

        public static string GetFormattedTime(float time, bool round = true)
        {
            if (Math.Abs(time - float.MaxValue) < 0.01f) return "N/A";

            if (round)
            {
                // round so that e.g. 18.401 (which fails an 18.40 time threshold) displays as 18.41
                time = Mathf.Ceil(time * 100f) / 100f;
            }
            else
            {
                // add small epsilon so that floating point error for exact values e.g. 18.40 doesn't bring the time down to 18.39
                time += 1e-6f;
            }
            
            var minutes = (int) (time / 60);
            var seconds = (int) (time % 60);
            var centiSeconds = (int) ((time - (int) time) * 100);

            return $"{minutes:00}:{seconds:00}:{centiSeconds:00}";
        }

        private void OnDestroy()
        {
            DroneTrackerService.OnDroneKilled -= HandleDroneKilled;
            DroneTrackerService.OnDroneKilledByGhost -= HandleDroneKilled;
        }
    }
}