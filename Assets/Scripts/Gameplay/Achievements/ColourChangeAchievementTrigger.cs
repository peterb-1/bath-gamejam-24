using Gameplay.Colour;
using Gameplay.Input;
using UnityEngine;

namespace Gameplay.Achievements
{
    public class ColourChangeAchievementTrigger : AbstractAchievementTrigger
    {
        [SerializeField] 
        private int colourChangesRequired;

        private int colourChanges;
        
        private void Awake()
        {
            InputManager.OnColourChanged += HandleColourChanged;
        }

        private void HandleColourChanged(ColourId _)
        {
            colourChanges++;

            if (colourChanges == colourChangesRequired)
            {
                TriggerAchievement();

                InputManager.OnColourChanged -= HandleColourChanged;
            }
        }

        private void OnDestroy()
        {
            InputManager.OnColourChanged -= HandleColourChanged;
        }
    }
}