using System;
using Core.Saving;
using Cysharp.Threading.Tasks;
using Gameplay.Input;
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

        public async UniTask OpenSettingsAsync(Action onClosedCallback)
        {
            settingsClosedCallback = onClosedCallback;
            pageGroup.SetDefaultPage();

            await pageGroup.ShowGroupAsync(isForward: false);

            InputManager.OnBackPerformed += HandleBackSelected;
        }

        private void HandleBackSelected()
        {
            InputManager.OnBackPerformed -= HandleBackSelected;

            SaveManager.Instance.Save();
            
            pageGroup.HideGroup(isForward: true);
            
            settingsClosedCallback?.Invoke();
        }

        private void HandleTabSelected(SettingsTabBehaviour tab)
        {
            pageGroup.SetPage(tab.Page);
        }
        
        private void OnDestroy()
        {
            InputManager.OnBackPerformed -= HandleBackSelected;
            
            backButton.onClick.RemoveListener(HandleBackSelected);
            
            foreach (var tab in tabs)
            {
                tab.OnTabSelected -= HandleTabSelected;
            }
        }
    }
}