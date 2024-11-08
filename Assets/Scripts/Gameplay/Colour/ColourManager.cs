using System;
using Cysharp.Threading.Tasks;
using Gameplay.Input;
using UnityEngine;

namespace Gameplay.Colour
{
    public class ColourManager : MonoBehaviour
    {
        [SerializeField] 
        private ColourId initialColour;

        [SerializeField] 
        private float colourChangeCooldown;
        
        [SerializeField] 
        private float colourChangeDuration;
        
        private ColourId currentColour;
        private bool canChangeColour;

        // subscribers to OnColourChangeStarted have a responsibility to complete their colour change transition within the specified duration
        public static event Action<ColourId, float> OnColourChangeStarted;
        public static event Action<ColourId> OnColourChangeInstant;
        public static event Action OnColourChangeEnded;

        private void Awake()
        {
            currentColour = initialColour;
            canChangeColour = true;

            InputManager.OnColourChanged += HandleColourChanged;
        }

        private void Start()
        {
            OnColourChangeInstant?.Invoke(currentColour);
        }

        private void HandleColourChanged(ColourId colour)
        {
            if (colour == currentColour || !canChangeColour) return;
            
            currentColour = colour;
            
            RunColourChangeAsync().Forget();
        }

        private async UniTask RunColourChangeAsync()
        {
            canChangeColour = false;
            
            OnColourChangeStarted?.Invoke(currentColour, colourChangeDuration);

            await UniTask.Delay(TimeSpan.FromSeconds(colourChangeCooldown));
            
            canChangeColour = true;

            await UniTask.Delay(TimeSpan.FromSeconds(colourChangeDuration - colourChangeCooldown));
            
            OnColourChangeEnded?.Invoke();
        }

        private void OnDestroy()
        {
            InputManager.OnColourChanged -= HandleColourChanged;
        }
    }
}
