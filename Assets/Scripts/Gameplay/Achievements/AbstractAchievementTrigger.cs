using System;
using UnityEngine;

namespace Gameplay.Achievements
{
    public abstract class AbstractAchievementTrigger : MonoBehaviour
    {
        public event Action<Achievement> OnAchievementUnlocked;

        private Achievement achievement;

        public void SetAchievement(Achievement a)
        {
            achievement = a;
        }

        protected void TriggerAchievement()
        {
            OnAchievementUnlocked?.Invoke(achievement);
        }
    }
}