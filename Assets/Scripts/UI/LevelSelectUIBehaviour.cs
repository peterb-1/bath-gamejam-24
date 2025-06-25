using System.Collections.Generic;
using Core;
using Core.Saving;
using Cysharp.Threading.Tasks;
using Gameplay.Core;
using Gameplay.Environment;
using Gameplay.Input;
using UnityEngine;
using UnityEngine.UI;

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

        private LevelConfig lastViewedLevelConfig;
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
                SceneConfig hiddenSceneConfig = null;
                
                foreach (var levelSelectButton in districtPage.LevelSelectButtons)
                {
                    var sceneConfig = levelSelectButton.SceneConfig;
                    var levelConfig = sceneConfig.LevelConfig;

                    if (SaveManager.Instance.SaveData.CampaignData.TryGetLevelData(levelConfig, out var levelData) && levelData.IsUnlocked)
                    {
                        if (levelConfig.IsHidden)
                        {
                            hiddenSceneConfig = sceneConfig;
                        }
                        else
                        {
                            orderedSceneConfigs.Add(sceneConfig);
                        }
                    }
                }

                if (hiddenSceneConfig != null)
                {
                    orderedSceneConfigs.Add(hiddenSceneConfig);
                }
            }
            
            leaderboardBehaviour.SetLevelConfigs(orderedSceneConfigs);
        }

        private void Start()
        {
            SetView(SceneLoader.Instance.PreviousSceneConfig);
            SetPageNavigation();
        }
        
        private void HandleSettingsClicked()
        {
            pageGroup.HideGroup(isForward: false);
            settingsBehaviour.OpenSettingsAsync(HandleSettingsClosed).Forget();
            
            AnimateCloudsAsync(isForward: false).Forget();
        }

        private void HandleLeaderboardClicked()
        {
            pageGroup.HideGroup(isForward: false);
            leaderboardBehaviour.OpenLeaderboardAsync(lastViewedLevelConfig, HandleLeaderboardClosed).Forget();
            
            AnimateCloudsAsync(isForward: false).Forget();
        }

        private async void HandleSettingsClosed()
        { 
            AnimateCloudsAsync(isForward: true).Forget();

            await pageGroup.ShowGroupAsync(isForward: true);

            if (InputManager.CurrentControlScheme is not ControlScheme.Mouse)
            {
                districtPages[currentPageIndex].SettingsButton.Select();
            }
        }

        private async void HandleLeaderboardClosed(SceneConfig viewedConfig)
        { 
            AnimateCloudsAsync(isForward: true).Forget();

            lastViewedLevelConfig = viewedConfig.LevelConfig;
            
            SetView(viewedConfig, districtPages[viewedConfig.LevelConfig.DistrictNumber - 1].LeaderboardButton);
            SetPageNavigation();

            await pageGroup.ShowGroupAsync(isForward: true);
        }

        private void SetView(SceneConfig sceneConfig, Selectable selectableOverride = null)
        {
            if (sceneConfig.IsLevelScene)
            {
                lastViewedLevelConfig = sceneConfig.LevelConfig;
            }

            if (sceneConfig != null && levelSelectButtonLookup.TryGetValue(sceneConfig, out var sceneConfigButtonData))
            {
                var (districtPage, levelSelectButton) = sceneConfigButtonData;
                
                pageGroup.SetPage(districtPage.Page);
                
                for (var i = 0; i < districtPages.Length; i++)
                {
                    if (districtPage == districtPages[i])
                    {
                        currentPageIndex = i;
                    }
                }

                if (selectableOverride != null)
                {
                    SetSelectableForTopNavigation(levelSelectButton);
                }
                else if (InputManager.CurrentControlScheme is not ControlScheme.Mouse)
                {
                    levelSelectButton.Select();
                }
            }
            else if (InputManager.CurrentControlScheme is not ControlScheme.Mouse && selectableOverride == null)
            {
                defaultButton.Select();
            }
            
            if (selectableOverride != null && InputManager.CurrentControlScheme is not ControlScheme.Mouse)
            {
                selectableOverride.Select();
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
                lastViewedLevelConfig = levelSelectButton.SceneConfig.LevelConfig;
                infoDisplayBehaviour.SetLevelInfoAsync(lastViewedLevelConfig).Forget();
            }
            
            SetSelectableForTopNavigation(button);
        }

        private void HandleBackHover(ExtendedButton button)
        {
            infoDisplayBehaviour.SetNextDistrict(currentPageIndex);
            
            SetSelectableForTopNavigation(button);
        }

        private void HandleForwardHover(ExtendedButton button)
        {
            infoDisplayBehaviour.SetNextDistrict(currentPageIndex + 2);
            
            SetSelectableForTopNavigation(button);
        }
        
        private void HandleButtonUnhover(ExtendedButton _)
        {
            infoDisplayBehaviour.SetNoData();
        }

        private void SetSelectableForTopNavigation(Selectable selectable)
        {
            districtPages[currentPageIndex].SetTopDownNavigation(selectable);
        }

        private async void HandleBackClicked()
        {
            currentPageIndex--;
            
            var newDistrictPage = districtPages[currentPageIndex];
            var leftmostButton = newDistrictPage.LevelSelectButtons[0];

            lastViewedLevelConfig = leftmostButton.SceneConfig.LevelConfig;
            
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
            var isForwardActive = currentPageIndex < districtPages.Length - 1 && 
                                  districtPages[currentPageIndex + 1].GetLeftmostUnlockedLevelButton() != null;
            
            lastViewedLevelConfig = newDistrictPage.LevelSelectButtons[0].SceneConfig.LevelConfig;

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
            backNavigation.selectOnUp = districtPage.LeaderboardButton;
            leftmostButton.navigation = leftmostNavigation;
            backButton.navigation = backNavigation;
            backButton.interactable = isBackActive;

            districtPage.SetTopLeftRightNavigation(
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