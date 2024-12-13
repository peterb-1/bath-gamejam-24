using UnityEngine;

namespace UI
{
    public class DistrictPageUIBehaviour : MonoBehaviour
    {
        [field: SerializeField]
        public Page Page { get; private set; }

        [field: SerializeField]
        public LevelSelectButton[] LevelSelectButtons { get; private set; }

        public LevelSelectButton GetLeftmostUnlockedLevelButton()
        {
            return LevelSelectButtons[0];
        }
        
        public LevelSelectButton GetRightmostUnlockedLevelButton()
        {
            return LevelSelectButtons[^1];
        }
    }
}