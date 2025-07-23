using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Settings
{
    public class SettingCarouselBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private TMP_Text label;
        
        [SerializeField] 
        private ExtendedButton leftButton;
        
        [SerializeField] 
        private ExtendedButton rightButton;

        private object[] options;
        private int index;

        public event Action<object> OnValueChanged;
        public event Action OnHover;
        public event Action OnUnhover;
        
        public Selectable[] Selectables => new Selectable[] { rightButton, leftButton };
        
        private void Awake()
        {
            leftButton.onClick.AddListener(OnLeftClicked);
            rightButton.onClick.AddListener(OnRightClicked);

            leftButton.OnHover += HandleHover;
            rightButton.OnHover += HandleHover;
            leftButton.OnUnhover += HandleUnhover;
            rightButton.OnUnhover += HandleUnhover;
        }

        private void OnLeftClicked()
        {
            index = (index - 1 + options.Length) % options.Length;
            UpdateLabel();
            
            OnValueChanged?.Invoke(options[index]);
        }

        private void OnRightClicked()
        {
            index = (index + 1) % options.Length;
            UpdateLabel();
            
            OnValueChanged?.Invoke(options[index]);
        }
        
        private void HandleHover(ExtendedButton _)
        {
            OnHover?.Invoke();
        }

        private void HandleUnhover(ExtendedButton _)
        {
            OnUnhover?.Invoke();
        }

        public void SetOptions(Array enumValues)
        {
            options = enumValues.Cast<object>().ToArray();
            index = 0;
            
            UpdateLabel();
        }

        public void SetCurrent(object value)
        {
            index = Array.IndexOf(options, value);
            
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            label.text = options[index].ToString();
        }
        
        private void OnDestroy()
        {
            leftButton.onClick.RemoveListener(OnLeftClicked);
            rightButton.onClick.RemoveListener(OnRightClicked);
            
            leftButton.OnHover -= HandleHover;
            rightButton.OnHover -= HandleHover;
            leftButton.OnUnhover -= HandleUnhover;
            rightButton.OnUnhover -= HandleUnhover;
        }
    }
}