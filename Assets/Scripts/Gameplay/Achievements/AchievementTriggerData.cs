using System;
using UnityEngine;

namespace Gameplay.Achievements
{
    [Serializable]
    public class AchievementTriggerData
    {
        [field: SerializeField]
        public Achievement Achievement { get; private set; }

        [field: SerializeField] 
        public AbstractAchievementTrigger Trigger { get; private set; }
    }
}