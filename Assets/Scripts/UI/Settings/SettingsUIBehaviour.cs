using System;
using Audio;
using Core.Saving;
using Cysharp.Threading.Tasks;
using Gameplay.Input;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Settings
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
        private int currentTabIndex = -1;

        private void Awake()
        {
            backButton.onClick.AddListener(HandleBackSelected);
            
            foreach (var tab in tabs)
            {
                tab.OnTabSelected += HandleTabSelected;
            }
        }

        public async UniTask OpenSettingsAsync(Action onClosedCallback)
        {
            settingsClosedCallback = onClosedCallback;
            
            HandleTabSelected(tabs[0]);

            await pageGroup.ShowGroupAsync(isForward: false);

            if (InputManager.CurrentControlScheme is not ControlScheme.Mouse)
            {
                tabs[0].TabButton.Select();
            }

            InputManager.OnBackPerformed += HandleBackPerformed;
        }

        private void HandleBackPerformed()
        {
            AudioManager.Instance.Play(AudioClipIdentifier.ButtonClick);
            
            HandleBackSelected();
        }

        private void HandleBackSelected()
        {
            InputManager.OnBackPerformed -= HandleBackPerformed;

            SaveManager.Instance.Save();
            
            pageGroup.HideGroup(isForward: true);
            
            settingsClosedCallback?.Invoke();
        }

        private void HandleTabSelected(SettingsTabBehaviour tab)
        {
            var oldTabIndex = currentTabIndex;
            
            for (var i = 0; i < tabs.Length; i++)
            {
                if (tabs[i] == tab)
                {
                    currentTabIndex = i;
                }
            }

            if (currentTabIndex == oldTabIndex) return;

            pageGroup.SetPage(tab.Page, isForward: currentTabIndex > oldTabIndex);

            foreach (var tabBehaviour in tabs)
            {
                tabBehaviour.SetTabDownNavigation(tab.FirstSelectable);
            }
        }
        
        private void OnDestroy()
        {
            InputManager.OnBackPerformed -= HandleBackPerformed;
            
            backButton.onClick.RemoveListener(HandleBackSelected);
            
            foreach (var tab in tabs)
            {
                tab.OnTabSelected -= HandleTabSelected;
            }
        }
    }
}