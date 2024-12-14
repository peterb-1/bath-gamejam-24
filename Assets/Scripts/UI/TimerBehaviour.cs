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
            return GetFormattedTime(TimeElapsed);
        }

        public static string GetFormattedTime(float time)
        {
            if (Math.Abs(time - float.MaxValue) < 0.01f) return "N/A";
            
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