using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SettingsUIBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private PageGroup pageGroup;

        [SerializeField] 
        private SettingsTabBehaviour[] tabs;

        [SerializeField] 
        private Button backButton;

        private Action settingsClosedCallback;

        private void Awake()
        {
            backButton.onClick.AddListener(HandleBackSelected);
            
            foreach (var tab in tabs)
            {
                tab.OnTabSelected += HandleTabSelected;
            }
        }

        public void OpenSettings(Action onClosedCallback)
        {
            pageGroup.SetDefaultPage();
            pageGroup.ShowGroup(isForward: false);

            settingsClosedCallback = onClosedCallback;
        }

        private void HandleBackSelected()
        {
            pageGroup.HideGroup(isForward: true);
            
            settingsClosedCallback?.Invoke();
        }

        private void HandleTabSelected(SettingsTabBehaviour tab)
        {
            pageGroup.SetPage(tab.Page);
        }
        
        private void OnDestroy()
        {
            backButton.onClick.RemoveListener(HandleBackSelected);
            
            foreach (var tab in tabs)
            {
                tab.OnTabSelected -= HandleTabSelected;
            }
        }
    }
}