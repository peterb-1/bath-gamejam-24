using System;
using Gameplay.Drone;
using UnityEngine;

namespace UI
{
    public class TimerBehaviour : MonoBehaviour
    {
        public float TimeElapsed { get; private set; }

        public event Action<float> OnTimeBonusApplied;

        private void Awake()
        {
            DroneTrackerService.OnDroneKilled += HandleDroneKilled;
        }

        private void Start()
        {
            TimeElapsed = 0f;
        }
        
        private void Update()
        {
            TimeElapsed += Time.deltaTime;
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
            // round so that e.g. 18.401 (which fails an 18.40 time threshold) displays as 18.41
            var roundedTime = Mathf.Ceil(TimeElapsed * 100f) / 100f;
            return GetFormattedTime(roundedTime);
        }

        public static string GetFormattedTime(float time)
        {
            if (Math.Abs(time - float.MaxValue) < 0.01f) return "N/A";

            // so that floating point error for exact values e.g. 18.40 doesn't bring the time down to 18.39
            time += 1e-6f;
            
            var minutes = (int) (time / 60);
            var seconds = (int) (time % 60);
            var centiSeconds = (int) ((time - (int) time) * 100);

            return $"{minutes:00}:{seconds:00}:{centiSeconds:00}";
        }

        private void OnDestroy()
        {
            DroneTrackerService.OnDroneKilled -= HandleDroneKilled;
        }
    }
}