using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SettingsTabBehaviour : MonoBehaviour
    {
        [field: SerializeField]
        public Page Page { get; private set; }

        [SerializeField] 
        private Button tabButton;

        public event Action<SettingsTabBehaviour> OnTabSelected;

        private void Awake()
        {
            tabButton.onClick.AddListener(HandleTabSelected);
        }

        private void HandleTabSelected()
        {
            OnTabSelected?.Invoke(this);
        }
        
        private void OnDestroy()
        {
            tabButton.onClick.RemoveListener(HandleTabSelected);
        }
    }
}