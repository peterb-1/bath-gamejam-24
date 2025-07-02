using System;
using Audio;
using Gameplay.Input;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class ExtendedToggle : Toggle
    {
        public event Action<ExtendedToggle> OnHover;
        public event Action<ExtendedToggle> OnUnhover;

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
            else
            {
                AudioManager.Instance.Play(AudioClipIdentifier.ButtonDenied);
            }
            
            base.OnPointerClick(eventData);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            if (interactable)
            {
                AudioManager.Instance.Play(AudioClipIdentifier.ButtonHover);
                OnHover?.Invoke(this);
            }
            
            base.OnSelect(eventData);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            OnUnhover?.Invoke(this);
            
            base.OnDeselect(eventData);
        }
        
        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (interactable && InputManager.CurrentControlScheme is ControlScheme.Mouse)
            {
                AudioManager.Instance.Play(AudioClipIdentifier.ButtonHover);
                OnHover?.Invoke(this);
            }
            
            base.OnPointerEnter(eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (InputManager.CurrentControlScheme is ControlScheme.Mouse)
            {
                OnUnhover?.Invoke(this);
            }
            
            base.OnPointerExit(eventData);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            InputManager.OnControlSchemeChanged -= HandleControlSchemeChanged;
        }
    }
}