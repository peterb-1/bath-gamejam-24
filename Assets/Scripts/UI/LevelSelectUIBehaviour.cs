﻿using System.Collections.Generic;
using Core;
using Core.Saving;
using Cysharp.Threading.Tasks;
using Gameplay.Core;
using Gameplay.Environment;
using Gameplay.Input;
using UnityEngine;

namespace UI
{
    public class LevelSelectUIBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private PageGroup pageGroup;

        [SerializeField] 
        private SettingsUIBehaviour settingsBehaviour;
        
        [SerializeField] 
        private LeaderboardUIBehaviour leaderboardBehaviour;

        [SerializeField] 
        private LevelSelectInfoDisplayUIBehaviour infoDisplayBehaviour;

        [SerializeField] 
        private DistrictPageUIBehaviour[] districtPages;

        [SerializeField] 
        private LevelSelectButton defaultButton;
        
        [SerializeField] 
        private ExtendedButton backButton;
        
        [SerializeField] 
        private ExtendedButton forwardButton;

        [SerializeField] 
        private CloudGroup cloudGroup;

        [SerializeField] 
        private AnimationCurve cloudAnimationCurve;
        
        [SerializeField] 
        private float cloudAnimationTime;

        [SerializeField]
        private float cloudAnimationStrength;

        private int currentPageIndex;

        private readonly Dictionary<SceneConfig, (DistrictPageUIBehaviour, LevelSelectButton)> levelSelectButtonLookup = new();

        private void Awake()
        {
            foreach (var districtPage in districtPages)
            {
                districtPage.OnSettingsClicked += HandleSettingsClicked;
                districtPage.OnLeaderboardClicked += HandleLeaderboardClicked;
                
                foreach (var levelSelectButton in districtPage.LevelSelectButtons)
                {
                    levelSelectButtonLookup.Add(levelSelectButton.SceneConfig, (districtPage, levelSelectButton));

                    levelSelectButton.OnHover += HandleLevelSelectButtonHover;
                    levelSelectButton.OnUnhover += HandleButtonUnhover;
                }
            }

            SceneLoader.OnSceneLoadStart += HandleSceneLoadStart;
            
            backButton.onClick.AddListener(HandleBackClicked);
            backButton.OnHover += HandleBackHover;
            backButton.OnUnhover += HandleButtonUnhover;
            
            forwardButton.onClick.AddListener(HandleForwardClicked);
            forwardButton.OnHover += HandleForwardHover;
            forwardButton.OnUnhover += HandleButtonUnhover;

            InitialiseDataForLeaderboardsAsync().Forget();
        }

        private async UniTask InitialiseDataForLeaderboardsAsync()
        {
            await UniTask.WaitUntil(() => SaveManager.IsReady);
            
            var orderedSceneConfigs = new List<SceneConfig>();
            
            foreach (var districtPage in districtPages)
            {
                foreach (var levelSelectButton in districtPage.LevelSelectButtons)
                {
                    var sceneConfig = levelSelectButton.SceneConfig;

                    if (SaveManager.Instance.SaveData.CampaignData.TryGetLevelData(sceneConfig.LevelConfig, out var levelData) && levelData.IsUnlocked)
                    {
                        orderedSceneConfigs.Add(sceneConfig);
                    }
                }
            }
            
            leaderboardBehaviour.SetLevelConfigs(orderedSceneConfigs);
        }

        private void Start()
        {
            SetPage();
            SetPageNavigation();
        }
        
        private void HandleSettingsClicked()
        {
            pageGroup.HideGroup(isForward: false);
            
            settingsBehaviour.OpenSettings(HandleMenuClosed);
            
            AnimateCloudsAsync(isForward: false).Forget();
        }

        private void HandleLeaderboardClicked()
        {
            pageGroup.HideGroup(isForward: false);
            
            leaderboardBehaviour.OpenLeaderboard(districtPages[currentPageIndex].LevelSelectButtons[0].SceneConfig.LevelConfig, HandleMenuClosed);
            
            AnimateCloudsAsync(isForward: false).Forget();
        }

        private void HandleMenuClosed()
        { 
            pageGroup.ShowGroup(isForward: true);
            
            AnimateCloudsAsync(isForward: true).Forget();
        }

        private void SetPage()
        {
            var previousSceneConfig = SceneLoader.Instance.PreviousSceneConfig;

            if (previousSceneConfig != null &&
                levelSelectButtonLookup.TryGetValue(previousSceneConfig, out var previousSceneConfigButtonData))
            {
                var (districtPage, levelSelectButton) = previousSceneConfigButtonData;
                
                pageGroup.SetPage(districtPage.Page);

                if (InputManager.CurrentControlScheme is not ControlScheme.Mouse)
                {
                    levelSelectButton.Select();
                }

                for (var i = 0; i < districtPages.Length; i++)
                {
                    if (districtPage == districtPages[i])
                    {
                        currentPageIndex = i;
                    }
                }
            }
            else if (InputManager.CurrentControlScheme is not ControlScheme.Mouse)
            {
                defaultButton.Select();
            }
        }
        
        private void HandleSceneLoadStart()
        {
            pageGroup.SetInteractable(false);
        }
        
        private void HandleLevelSelectButtonHover(ExtendedButton button)
        {
            if (button is LevelSelectButton levelSelectButton)
            {
                infoDisplayBehaviour.SetLevelInfoAsync(levelSelectButton.SceneConfig).Forget();
            }
        }

        private void HandleBackHover(ExtendedButton _)
        {
            infoDisplayBehaviour.SetNextDistrict(currentPageIndex);
        }

        private void HandleForwardHover(ExtendedButton _)
        {
            infoDisplayBehaviour.SetNextDistrict(currentPageIndex + 2);
        }
        
        private void HandleButtonUnhover(ExtendedButton _)
        {
            infoDisplayBehaviour.SetNoData();
        }

        private async void HandleBackClicked()
        {
            currentPageIndex--;
            
            var newDistrictPage = districtPages[currentPageIndex];
            var leftmostButton = newDistrictPage.LevelSelectButtons[0];
            
            SetPageNavigation();
            
            AnimateCloudsAsync(isForward: false).Forget();

            if (currentPageIndex > 0)
            {
                infoDisplayBehaviour.SetNextDistrict(currentPageIndex);
            }
            else if (InputManager.CurrentControlScheme is ControlScheme.Mouse)
            {
                infoDisplayBehaviour.SetNoData();
            }
            
            await pageGroup.SetPageAsync(newDistrictPage.Page, isForward: false);
            
            // may have gone to a new page while waiting for the transition to complete
            if (districtPages[currentPageIndex] != newDistrictPage) return;

            if (currentPageIndex == 0 && InputManager.CurrentControlScheme is not ControlScheme.Mouse)
            {
                leftmostButton.Select();
            }
        }
        
        private async void HandleForwardClicked()
        {
            currentPageIndex++;
            
            var newDistrictPage = districtPages[currentPageIndex];
            var rightmostButton = newDistrictPage.GetRightmostUnlockedLevelButton() ?? backButton;
            var areAllLevelsUnlocked = rightmostButton == newDistrictPage.LevelSelectButtons[^1];
            var isForwardActive = currentPageIndex < districtPages.Length - 1 && areAllLevelsUnlocked;

            SetPageNavigation();
            
            AnimateCloudsAsync(isForward: true).Forget();
            
            if (isForwardActive)
            {
                infoDisplayBehaviour.SetNextDistrict(currentPageIndex + 2);
            }
            else if (InputManager.CurrentControlScheme is ControlScheme.Mouse)
            {
                infoDisplayBehaviour.SetNoData();
            }

            await pageGroup.SetPageAsync(newDistrictPage.Page, isForward: true);

            // may have gone to a new page while waiting for the transition to complete
            if (districtPages[currentPageIndex] != newDistrictPage) return;

            if (!isForwardActive && InputManager.CurrentControlScheme is not ControlScheme.Mouse)
            {
                rightmostButton.Select();
            }
        }

        private void SetPageNavigation()
        {
            var districtPage = districtPages[currentPageIndex];
            var leftmostButton = districtPage.GetLeftmostUnlockedLevelButton() ?? forwardButton;
            var rightmostLevelButton = districtPage.GetRightmostUnlockedLevelButton();
            var areAnyLevelsUnlocked = rightmostLevelButton != null;
            var rightmostButton = areAnyLevelsUnlocked ? rightmostLevelButton : backButton;

            var leftmostNavigation = leftmostButton.navigation;
            var rightmostNavigation = rightmostButton.navigation;
            var backNavigation = backButton.navigation;
            var forwardNavigation = forwardButton.navigation;
            var isBackActive = currentPageIndex > 0;
            var isForwardActive = currentPageIndex < districtPages.Length - 1 &&
                                  districtPages[currentPageIndex + 1].GetLeftmostUnlockedLevelButton() != null;
            
            rightmostNavigation.selectOnRight = isForwardActive ? forwardButton : null;
            forwardNavigation.selectOnLeft = rightmostButton;
            forwardNavigation.selectOnUp = districtPage.SettingsButton;
            rightmostButton.navigation = rightmostNavigation;
            forwardButton.navigation = forwardNavigation;
            forwardButton.interactable = isForwardActive;
            
            leftmostNavigation.selectOnLeft = isBackActive ? backButton : null;
            backNavigation.selectOnRight = leftmostButton;
            backNavigation.selectOnUp = districtPage.SettingsButton;
            leftmostButton.navigation = leftmostNavigation;
            backButton.navigation = backNavigation;
            backButton.interactable = isBackActive;

            districtPage.SetSettingsNavigation(
                isBackActive ? backButton : leftmostButton, 
                isForwardActive ? forwardButton : rightmostButton);
        }

        private async UniTask AnimateCloudsAsync(bool isForward)
        {
            var speedScale = isForward ? 1.0f : -1.0f;
            var baseSpeed = cloudGroup.BaseSpeed;
            var startSpeed = cloudGroup.CurrentSpeed;
            var speedDifference = Mathf.Abs(startSpeed - baseSpeed) / cloudAnimationStrength;
            var halfTime = cloudAnimationTime * 0.5f;
            var initialTime = TimeManager.Instance.UnpausedRealtimeSinceStartup;
            var timeElapsed = 0.0f;
            
            while (timeElapsed < halfTime)
            {
                var lerp = cloudAnimationCurve.Evaluate(timeElapsed / halfTime);
                var speed = startSpeed + cloudAnimationStrength * lerp * speedScale * (1 - speedDifference);
                var clampedSpeed = isForward
                    ? Mathf.Max(speed, baseSpeed + cloudAnimationStrength)
                    : Mathf.Min(speed, baseSpeed - cloudAnimationStrength);
                
                cloudGroup.SetSpeed(clampedSpeed);
                
                await UniTask.Yield();
                
                timeElapsed = TimeManager.Instance.UnpausedRealtimeSinceStartup - initialTime;
            }

            initialTime = TimeManager.Instance.UnpausedRealtimeSinceStartup;
            timeElapsed = 0.0f;
            
            while (timeElapsed < halfTime)
            {
                var lerp = 1.0f - cloudAnimationCurve.Evaluate(timeElapsed / halfTime);
                var speed = baseSpeed + cloudAnimationStrength * lerp * speedScale;
                var clampedSpeed = isForward
                    ? Mathf.Max(speed, baseSpeed + cloudAnimationStrength)
                    : Mathf.Min(speed, baseSpeed - cloudAnimationStrength);
                
                cloudGroup.SetSpeed(clampedSpeed);

                cloudGroup.SetSpeed(speed);
                
                await UniTask.Yield();
                
                timeElapsed = TimeManager.Instance.UnpausedRealtimeSinceStartup - initialTime;
            }
            
            cloudGroup.SetSpeed(baseSpeed);
        }
        
        private void OnDestroy()
        {
            foreach (var districtPage in districtPages)
            {
                districtPage.OnSettingsClicked -= HandleSettingsClicked;
                districtPage.OnLeaderboardClicked -= HandleLeaderboardClicked;
                
                foreach (var levelSelectButton in districtPage.LevelSelectButtons)
                {
                    levelSelectButton.OnHover -= HandleLevelSelectButtonHover;
                    levelSelectButton.OnUnhover -= HandleButtonUnhover;
                }
            }
            
            SceneLoader.OnSceneLoadStart -= HandleSceneLoadStart;
            
            backButton.onClick.RemoveListener(HandleBackClicked);
            backButton.OnHover -= HandleBackHover;
            backButton.OnUnhover -= HandleButtonUnhover;
            
            forwardButton.onClick.RemoveListener(HandleForwardClicked);
            forwardButton.OnHover -= HandleForwardHover;
            forwardButton.OnUnhover -= HandleButtonUnhover;
        }
    }
}