using System;
using Audio;
using Core.Saving;
using Cysharp.Threading.Tasks;
using Gameplay.Ghosts;
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
            GhostRunner.OnGhostColourChangedWhileSpectating += HandleGhostColourChangedWhileSpectating;
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

        private void HandleGhostColourChangedWhileSpectating(ColourId colour)
        {
            if (colour == currentColour) return;
            
            currentColour = colour;
            
            RunColourChangeAsync(isSpectating: true).Forget();
        }

        private async UniTask RunColourChangeAsync(bool isSpectating = false)
        {
            canChangeColour = false;
            
            AudioManager.Instance.Play(AudioClipIdentifier.ColourSwitch);
            
            if (!isSpectating)
            {
                SaveManager.Instance.SaveData.StatsData.AddToStat(StatType.ColourChanges, 1);
            }
            
            OnColourChangeStarted?.Invoke(currentColour, colourChangeDuration);

            await UniTask.Delay(TimeSpan.FromSeconds(colourChangeCooldown));
            
            canChangeColour = true;

            await UniTask.Delay(TimeSpan.FromSeconds(colourChangeDuration - colourChangeCooldown));
            
            OnColourChangeEnded?.Invoke();
        }

        private void OnDestroy()
        {
            InputManager.OnColourChanged -= HandleColourChanged;
            GhostRunner.OnGhostColourChangedWhileSpectating -= HandleGhostColourChangedWhileSpectating;
        }
    }
}
