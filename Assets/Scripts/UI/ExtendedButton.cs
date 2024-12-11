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
            base.OnSubmit(eventData);
            
            AudioManager.Instance.Play(AudioClipIdentifier.ButtonClick);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            
            AudioManager.Instance.Play(AudioClipIdentifier.ButtonClick);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            InputManager.OnControlSchemeChanged -= HandleControlSchemeChanged;
        }
    }
}
