using System;
using Cysharp.Threading.Tasks;
using Gameplay.Input;
using UnityEngine;
using Utils;

namespace Gameplay.Colour
{
    public class ColourManager : MonoBehaviour
    {
        [SerializeField] 
        private float colourChangeDuration;
        
        public ColourId CurrentColour { get; private set; }
        
        public static ColourManager Instance { get; private set; }

        // subscribers to OnColourChangeStarted have a responsibility to complete their colour change transition within the specified duration
        public static event Action<ColourId, float> OnColourChangeStarted;
        public static event Action<ColourId> OnColourChangeInstant;
        public static event Action OnColourChangeEnded;

        private bool isChangingColour;
        
        private void Awake()
        {
            if (Instance != null)
            {
                GameLogger.LogWarning("There should only be one ColourManager in the scene! Destroying this one.", this);
                Destroy(gameObject);
            }
            
            Instance = this;

            CurrentColour = ColourId.Blue;

            InputManager.OnColourChanged += HandleColourChanged;
        }

        private void Start()
        {
            OnColourChangeInstant?.Invoke(CurrentColour);
        }

        private void HandleColourChanged(ColourId colour)
        {
            if (colour == CurrentColour || isChangingColour) return;
            
            CurrentColour = colour;
            
            RunColourChangeAsync().Forget();
        }

        private async UniTask RunColourChangeAsync()
        {
            isChangingColour = true;
            
            OnColourChangeStarted?.Invoke(CurrentColour, colourChangeDuration);

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
