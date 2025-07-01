using System;
using NaughtyAttributes;
using TMPro;
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

        [SerializeField] 
        private bool overrideSettingDisplay;
        
        [SerializeField, HideIf(nameof(overrideSettingDisplay))] 
        private PageGroup pageGroup;

        [SerializeField, HideIf(nameof(overrideSettingDisplay))] 
        private Page noDataPage;
        
        [SerializeField, HideIf(nameof(overrideSettingDisplay))] 
        private Page infoPage;

        [SerializeField, HideIf(nameof(overrideSettingDisplay))] 
        private TMP_Text settingNameText;
        
        [SerializeField, HideIf(nameof(overrideSettingDisplay))] 
        private TMP_Text settingDescriptionText;

        [SerializeField, HideIf(nameof(overrideSettingDisplay))]
        private AbstractSettingDisplay[] settingDisplays;

        public event Action<SettingsTabBehaviour> OnTabSelected;

        private void Awake()
        {
            tabButton.onClick.AddListener(HandleTabSelected);

            foreach (var settingDisplay in settingDisplays)
            {
                settingDisplay.OnHover += HandleSettingHover;
                settingDisplay.OnUnhover += HandleSettingUnhover;
            }
        }

        private void HandleTabSelected()
        {
            OnTabSelected?.Invoke(this);
        }
        
        private void HandleSettingHover(AbstractSettingDisplay settingDisplay)
        {
            pageGroup.SetPage(infoPage);
            
            settingNameText.text = settingDisplay.SettingName;
            settingDescriptionText.text = settingDisplay.SettingDescription;
        }
        
        private void HandleSettingUnhover()
        {
            pageGroup.SetPage(noDataPage);
        }
        
        private void OnDestroy()
        {
            tabButton.onClick.RemoveListener(HandleTabSelected);
            
            foreach (var settingDisplay in settingDisplays)
            {
                settingDisplay.OnHover -= HandleSettingHover;
                settingDisplay.OnUnhover -= HandleSettingUnhover;
            }
        }
    }
}