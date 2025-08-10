using System;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UI.Trails;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Settings
{
    public class SettingsTabBehaviour : MonoBehaviour
    {
        [field: SerializeField]
        public Page Page { get; private set; }

        [SerializeField] 
        private Button tabButton;

        [SerializeField] 
        private bool overrideSettingDisplay;
        
        [SerializeField, ShowIf(nameof(overrideSettingDisplay))] 
        private TrailSelectionUIBehaviour trailSelectionBehaviour;
        
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

        public Button TabButton => tabButton;
        
        public Selectable FirstSelectable => overrideSettingDisplay 
            ? trailSelectionBehaviour.FirstSelectable 
            : settingDisplays[0].GetSelectables()[0];

        private readonly List<AbstractSettingDisplay> hoveredDisplays = new();

        public event Action<SettingsTabBehaviour> OnTabSelected;

        private void Awake()
        {
            tabButton.onClick.AddListener(HandleTabSelected);

            foreach (var settingDisplay in settingDisplays)
            {
                settingDisplay.OnHover += HandleSettingHover;
                settingDisplay.OnUnhover += HandleSettingUnhover;
            }
            
            SetNavigation();
        }

        private void HandleTabSelected()
        {
            OnTabSelected?.Invoke(this);
        }
        
        private void HandleSettingHover(AbstractSettingDisplay settingDisplay)
        {
            if (hoveredDisplays.Count == 0)
            {
                DisplaySettingInfo(settingDisplay);
            }
            
            hoveredDisplays.Add(settingDisplay);
        }
        
        private void HandleSettingUnhover(AbstractSettingDisplay settingDisplay)
        {
            hoveredDisplays.Remove(settingDisplay);

            if (hoveredDisplays.Count > 0)
            {
                DisplaySettingInfo(hoveredDisplays[0]);
            }
            else
            {
                pageGroup.SetPage(noDataPage);
            }
        }

        private void DisplaySettingInfo(AbstractSettingDisplay settingDisplay)
        {
            pageGroup.SetPage(infoPage);
            
            settingNameText.text = settingDisplay.SettingName;
            settingDescriptionText.text = settingDisplay.SettingDescription;
        }
        
        private void SetNavigation()
        {
            for (var i = 0; i < settingDisplays.Length - 1; i++)
            {
                var currentSelectables = settingDisplays[i].GetSelectables();
                var nextSelectables = settingDisplays[i + 1].GetSelectables();

                for (var j = Mathf.Max(currentSelectables.Length, nextSelectables.Length) - 1; j >= 0; j--)
                {
                    var currentSelectable = currentSelectables[Mathf.Min(j, currentSelectables.Length - 1)];
                    var nextSelectable = nextSelectables[Mathf.Min(j, nextSelectables.Length - 1)];
                    var currentNavigation = currentSelectable.navigation;
                    var nextNavigation = nextSelectable.navigation;

                    currentNavigation.selectOnDown = nextSelectable;
                    nextNavigation.selectOnUp = currentSelectable;
                
                    if (i == 0)
                    {
                        currentNavigation.selectOnUp = tabButton;
                    }
                
                    currentSelectable.navigation = currentNavigation;
                    nextSelectable.navigation = nextNavigation;
                }
            }
        }

        public void SetTabDownNavigation(Selectable selectOnDown)
        {
            var tabNavigation = tabButton.navigation;
            tabNavigation.selectOnDown = selectOnDown;
            tabButton.navigation = tabNavigation;
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