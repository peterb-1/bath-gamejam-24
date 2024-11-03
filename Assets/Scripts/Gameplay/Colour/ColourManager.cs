using System;
using Cysharp.Threading.Tasks;
using Gameplay.Input;
using UnityEngine;

namespace Gameplay.Colour
{
    public class ColourManager : MonoBehaviour
    {
        [SerializeField] 
        private float colourChangeDuration;
        
        private ColourId currentColour;
        private bool isChangingColour;

        // subscribers to OnColourChangeStarted have a responsibility to complete their colour change transition within the specified duration
        public static event Action<ColourId, float> OnColourChangeStarted;
        public static event Action<ColourId> OnColourChangeInstant;
        public static event Action OnColourChangeEnded;

        private void Awake()
        {
            currentColour = ColourId.Blue;

            InputManager.OnColourChanged += HandleColourChanged;
        }

        private void Start()
        {
            OnColourChangeInstant?.Invoke(currentColour);
        }

        private void HandleColourChanged(ColourId colour)
        {
            if (colour == currentColour || isChangingColour) return;
            
            currentColour = colour;
            
            RunColourChangeAsync().Forget();
        }

        private async UniTask RunColourChangeAsync()
        {
            isChangingColour = true;
            
            OnColourChangeStarted?.Invoke(currentColour, colourChangeDuration);

            await UniTask.Delay(TimeSpan.FromSeconds(colourChangeDuration));
            
            OnColourChangeEnded?.Invoke();
            
            isChangingColour = false;
        }

        private void OnDestroy()
        {
            InputManager.OnColourChanged -= HandleColourChanged;
        }
    }
}
