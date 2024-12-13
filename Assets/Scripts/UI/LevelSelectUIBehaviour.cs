using System.Collections.Generic;
using Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LevelSelectUIBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private PageGroup pageGroup;

        [SerializeField] 
        private DistrictPageUIBehaviour[] districtPages;

        [SerializeField] 
        private LevelSelectButton defaultButton;
        
        [SerializeField] 
        private Button backButton;
        
        [SerializeField] 
        private Button forwardButton;

        private int currentPageIndex;

        private readonly Dictionary<SceneConfig, (DistrictPageUIBehaviour, LevelSelectButton)> levelSelectButtonLookup = new();

        private void Awake()
        {
            foreach (var districtPage in districtPages)
            {
                foreach (var levelSelectButton in districtPage.LevelSelectButtons)
                {
                    levelSelectButtonLookup.Add(levelSelectButton.SceneConfig, (districtPage, levelSelectButton));
                }
            }

            SceneLoader.OnSceneLoadStart += HandleSceneLoadStart;
            
            backButton.onClick.AddListener(HandleBackClicked);
            forwardButton.onClick.AddListener(HandleForwardClicked);
        }

        private void Start()
        {
            SelectDefaultSelectable();
            SetPageNavigation();
        }

        private void SelectDefaultSelectable()
        {
            var previousSceneConfig = SceneLoader.Instance.PreviousSceneConfig;

            if (previousSceneConfig == null || !levelSelectButtonLookup.TryGetValue(
                    previousSceneConfig, out var previousSceneConfigButtonData))
            {
                defaultButton.Select();
            }
            else
            {
                var (districtPage, levelSelectButton) = previousSceneConfigButtonData;
                
                pageGroup.SetPage(districtPage.Page);
                levelSelectButton.Select();

                for (var i = 0; i < districtPages.Length; i++)
                {
                    if (districtPage == districtPages[i])
                    {
                        currentPageIndex = i;
                    }
                }
            }
        }
        
        private void HandleBackClicked()
        {
            currentPageIndex--;
            
            var newDistrictPage = districtPages[currentPageIndex];
            var leftmostButton = newDistrictPage.GetLeftmostUnlockedLevelButton();
            
            SetPageNavigation();
            
            pageGroup.SetPage(newDistrictPage.Page, isForward: false);

            if (currentPageIndex == 0)
            {
                leftmostButton.Select();
            }
        }
        
        private void HandleForwardClicked()
        {
            currentPageIndex++;
            
            var newDistrictPage = districtPages[currentPageIndex];
            var rightmostButton = newDistrictPage.GetRightmostUnlockedLevelButton();

            SetPageNavigation();
            
            pageGroup.SetPage(newDistrictPage.Page, isForward: true);

            if (currentPageIndex == districtPages.Length - 1)
            {
                rightmostButton.Select();
            }
        }

        private void SetPageNavigation()
        {
            var districtPage = districtPages[currentPageIndex];
            var leftmostButton = districtPage.GetLeftmostUnlockedLevelButton();
            var leftmostNavigation = leftmostButton.navigation;
            var rightmostButton = districtPage.GetRightmostUnlockedLevelButton();
            var rightmostNavigation = rightmostButton.navigation;
            var backNavigation = backButton.navigation;
            var isBackActive = currentPageIndex > 0;
            var forwardNavigation = forwardButton.navigation;
            var isForwardActive = currentPageIndex < districtPages.Length - 1;
                
            leftmostNavigation.selectOnLeft = isBackActive ? backButton : null;
            backNavigation.selectOnRight = leftmostButton;
            leftmostButton.navigation = leftmostNavigation;
            backButton.navigation = backNavigation;
            backButton.interactable = isBackActive;
            
            rightmostNavigation.selectOnRight = isForwardActive ? forwardButton : null;
            forwardNavigation.selectOnLeft = rightmostButton;
            rightmostButton.navigation = rightmostNavigation;
            forwardButton.navigation = forwardNavigation;
            forwardButton.interactable = isForwardActive;
        }

        private void HandleSceneLoadStart()
        {
            pageGroup.HideGroup();
        }
        
        private void OnDestroy()
        {
            SceneLoader.OnSceneLoadStart -= HandleSceneLoadStart;
            
            backButton.onClick.RemoveListener(HandleBackClicked);
            forwardButton.onClick.RemoveListener(HandleForwardClicked);
        }
    }
}