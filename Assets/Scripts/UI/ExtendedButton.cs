using System;
using Audio;
using Gameplay.Input;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class ExtendedButton : Button
    {
        protected override void Awake()
        {
            base.Awake();

            InputManager.OnControlSchemeChanged += HandleControlSchemeChanged;
            
            HandleControlSchemeChanged(InputManager.CurrentControlScheme);
        }

        private void HandleControlSchemeChanged(ControlScheme controlScheme)
        {
            var navigationMode = controlScheme switch
            {
                ControlScheme.Keyboard or ControlScheme.Gamepad => Navigation.Mode.Explicit,
                ControlScheme.Mouse => Navigation.Mode.None,
                _ => throw new ArgumentOutOfRangeException(nameof(controlScheme), controlScheme, null)
            };

            var tempNavigation = navigation;
            tempNavigation.mode = navigationMode;
            navigation = tempNavigation;
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            // play sfx before calling base method in case clicking the button deactivates it
            if (interactable)
            {
                AudioManager.Instance.Play(AudioClipIdentifier.ButtonClick);
            }
            
            base.OnSubmit(eventData);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            // play sfx before calling base method in case clicking the button deactivates it
            if (interactable)
            {
                AudioManager.Instance.Play(AudioClipIdentifier.ButtonClick);
            }
            
            base.OnPointerClick(eventData);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            InputManager.OnControlSchemeChanged -= HandleControlSchemeChanged;
        }
    }
}
