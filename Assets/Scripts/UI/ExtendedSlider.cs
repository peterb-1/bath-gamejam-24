using System;
using Audio;
using Gameplay.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class ExtendedSlider : Slider
    {
        private bool isDragging;
        
        public event Action<ExtendedSlider> OnHover;
        public event Action<ExtendedSlider> OnUnhover;

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
            if (!isDragging && InputManager.CurrentControlScheme is ControlScheme.Mouse)
            {
                OnUnhover?.Invoke(this);
            }
            
            base.OnPointerExit(eventData);
        }
        
        public override void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                isDragging = true;
            }
            
            base.OnPointerDown(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                isDragging = false;
                
                if (InputManager.CurrentControlScheme is ControlScheme.Mouse &&
                    !RectTransformUtility.RectangleContainsScreenPoint(
                        transform as RectTransform,
                        eventData.position,
                        eventData.pressEventCamera))
                {
                    OnUnhover?.Invoke(this);
                }
            }
            
            base.OnPointerUp(eventData);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            InputManager.OnControlSchemeChanged -= HandleControlSchemeChanged;
        }
    }
}