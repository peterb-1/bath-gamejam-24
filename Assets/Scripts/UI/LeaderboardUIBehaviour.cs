﻿using System;
using System.Collections.Generic;
using System.Threading;
using Audio;
using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Core;
using Gameplay.Input;
using Steam;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class LeaderboardUIBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private PageGroup pageGroup;

        [SerializeField] 
        private TMP_Text leaderboardTitleText;
        
        [SerializeField] 
        private TMP_Text uploadInfoText;

        [SerializeField] 
        private GameObject errorMessage;
        
        [SerializeField] 
        private GameObject noEntriesMessage;

        [SerializeField] 
        private LoadingSpinner loadingSpinner;

        [SerializeField] 
        private Button previousButton;

        [SerializeField] 
        private Button nextButton;
        
        [SerializeField] 
        private Button globalButton;
        
        [SerializeField] 
        private Button friendsButton;
        
        [SerializeField] 
        private Button refreshButton;

        [SerializeField] 
        private Button backButton;

        [SerializeField] 
        private LeaderboardRow[] leaderboardRows;

        private CancellationTokenSource leaderboardCancellationTokenSource;
        private List<SceneConfig> orderedSceneConfigs;
        private ELeaderboardDataRequest currentRequestType;
        private Action<SceneConfig> settingsClosedCallback;
        private int currentConfigIndex;

        private void Awake()
        {
            currentRequestType = ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal;
            
            previousButton.onClick.AddListener(HandlePreviousSelected);
            nextButton.onClick.AddListener(HandleNextSelected);
            globalButton.onClick.AddListener(HandleGlobalSelected);
            friendsButton.onClick.AddListener(HandleFriendsSelected);
            refreshButton.onClick.AddListener(HandleRefreshSelected);
            backButton.onClick.AddListener(HandleBackSelected);

            foreach (var row in leaderboardRows)
            {
                row.OnClickedPlay += HandleClickedPlay;
                row.OnDownloadStarted += HandleDownloadStarted;
                row.OnDownloadFinished += HandleDownloadFinished;
            }
        }
        
        private void Update()
        {
            if (!SteamLeaderboards.IsReady()) return;
            
            var queueSize = SteamLeaderboards.Instance.UploadsQueued;
            var levelConfig = SteamLeaderboards.Instance.CurrentlyProcessedLevelConfig;

            uploadInfoText.text = (queueSize, levelConfig) switch
            {
                (_, not null) => $"Uploading score for mission {levelConfig.GetLevelCode()}...",
                (0, null) => "All scores uploaded!",
                (1, null) => "1 score pending upload...",
                (> 1, null) => $"{queueSize} scores pending upload...",
                _ => ""
            };
        }

        public void SetLevelConfigs(List<SceneConfig> sceneConfigs)
        {
            orderedSceneConfigs = sceneConfigs;
        }

        public async UniTask OpenLeaderboardAsync(LevelConfig levelConfig, Action<SceneConfig> onClosedCallback)
        {
            levelConfig ??= orderedSceneConfigs[0].LevelConfig;
            
            for (var i = 0; i < orderedSceneConfigs.Count; i++)
            {
                if (orderedSceneConfigs[i].LevelConfig.Guid == levelConfig.Guid)
                {
                    currentConfigIndex = i;
                }
            }

            PopulateLeaderboard(currentRequestType);

            settingsClosedCallback = onClosedCallback;
            pageGroup.SetDefaultPage();

            await pageGroup.ShowGroupAsync(isForward: false);

            InputManager.OnBackPerformed += HandleBackPerformed;
        }

        private void PopulateLeaderboard(ELeaderboardDataRequest requestType)
        {
            currentRequestType = requestType;
            
            leaderboardCancellationTokenSource?.Cancel();
            leaderboardCancellationTokenSource?.Dispose();
            leaderboardCancellationTokenSource = new CancellationTokenSource();
            
            PopulateLeaderboardAsync(leaderboardCancellationTokenSource.Token, requestType).Forget();
        }

        private async UniTask PopulateLeaderboardAsync(CancellationToken token, ELeaderboardDataRequest requestType)
        {
            foreach (var row in leaderboardRows)
            {
                row.gameObject.SetActive(false);
            }
            
            errorMessage.SetActive(false);
            noEntriesMessage.SetActive(false);

            var sceneConfig = orderedSceneConfigs[currentConfigIndex];
            var levelConfig = sceneConfig.LevelConfig;
            
            leaderboardTitleText.text = levelConfig.GetLevelText();
            
            loadingSpinner.gameObject.SetActive(true);
            loadingSpinner.StartSpinner();

            await UniTask.WaitUntil(SteamLeaderboards.IsReady, cancellationToken: token);

            var (wasCancelled, (result, entries)) = await SteamLeaderboards.Instance.TryGetLeaderboardScoresAsync(
                    levelConfig, 
                    requestType, 
                    1, 
                    leaderboardRows.Length)
                .AttachExternalCancellation(token)
                .SuppressCancellationThrow();

            loadingSpinner.StopSpinner();
            loadingSpinner.gameObject.SetActive(false);

            if (wasCancelled) return;

            errorMessage.SetActive(result == LeaderboardResultStatus.Failure);
            noEntriesMessage.SetActive(result == LeaderboardResultStatus.NoEntries);

            if (entries == null) return;

            for (var i = 0; i < Math.Min(entries.Count, leaderboardRows.Length); i++)
            {
                var row = leaderboardRows[i];
                var entry = entries[i].Entry;
                var details = entries[i].Details;
                var fileId = details.Length >= 2 ? ((ulong) details[1] << 32) | (uint) details[0] : 0;
                
                row.gameObject.SetActive(true);
                row.SetDetails(entry.m_nGlobalRank, entry.m_steamIDUser, entry.m_nScore, fileId, sceneConfig);
            }
            
            SetNavigation();
        }

        private void HandlePreviousSelected()
        {
            currentConfigIndex = MathsUtils.Modulo(currentConfigIndex - 1, orderedSceneConfigs.Count);
            PopulateLeaderboard(currentRequestType);
        }

        private void HandleNextSelected()
        {
            currentConfigIndex = MathsUtils.Modulo(currentConfigIndex + 1, orderedSceneConfigs.Count);
            PopulateLeaderboard(currentRequestType);
        }
        
        private void HandleGlobalSelected()
        {
            if (currentRequestType == ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal) return;
            PopulateLeaderboard(ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal);
        }
        
        private void HandleFriendsSelected()
        {
            if (currentRequestType == ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends) return;
            PopulateLeaderboard(ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends);
        }

        private void HandleRefreshSelected()
        {
            PopulateLeaderboard(currentRequestType);
        }
        
        private void HandleBackPerformed()
        {
            AudioManager.Instance.Play(AudioClipIdentifier.ButtonClick);
            
            HandleBackSelected();
        }

        private void HandleBackSelected()
        {
            InputManager.OnBackPerformed -= HandleBackPerformed;
            
            // UI system doesn't do this automatically because the back button isn't on the page, it's in the shared elements
            EventSystem.current.SetSelectedGameObject(null);
            
            leaderboardCancellationTokenSource?.Cancel();
            pageGroup.HideGroup(isForward: true);
            settingsClosedCallback?.Invoke(orderedSceneConfigs[currentConfigIndex]);
        }
        
        private void HandleClickedPlay()
        {
            pageGroup.SetInteractable(false);
        }
        
        private void HandleDownloadStarted()
        {
            previousButton.interactable = false;
            nextButton.interactable = false;
            globalButton.interactable = false;
            friendsButton.interactable = false;
            refreshButton.interactable = false;

            var backNavigation = backButton.navigation;
            backNavigation.selectOnRight = null;
            backNavigation.selectOnDown = null;
            backButton.navigation = backNavigation;

            foreach (var row in leaderboardRows)
            {
                row.DisableDownloads();
            }
        }
        
        private void HandleDownloadFinished()
        {
            previousButton.interactable = true;
            nextButton.interactable = true;
            globalButton.interactable = true;
            friendsButton.interactable = true;
            refreshButton.interactable = true;
            
            var backNavigation = backButton.navigation;
            backNavigation.selectOnRight = previousButton;
            backNavigation.selectOnDown = leaderboardRows[0].GhostButton;
            backButton.navigation = backNavigation;
            
            foreach (var row in leaderboardRows)
            {
                row.EnableDownloads();
            }
            
            SetNavigation();
        }

        private void SetNavigation()
        {
            var firstRow = leaderboardRows[0];
            var friendsNavigation = friendsButton.navigation;
            
            friendsNavigation.selectOnDown = firstRow.SpectateButton.isActiveAndEnabled
                ? firstRow.SpectateButton
                : firstRow.GhostButton;
            
            friendsButton.navigation = friendsNavigation;
            
            for (var i = 0; i < leaderboardRows.Length - 1; i++)
            {
                var upperRow = leaderboardRows[i];
                var lowerRow = leaderboardRows[i + 1];

                if (!lowerRow.gameObject.activeSelf) break;

                upperRow.SetDownNavigation(lowerRow.GhostButton, lowerRow.SpectateButton, lowerRow.SpectateButton.isActiveAndEnabled);
                lowerRow.SetUpNavigation(upperRow.GhostButton, upperRow.SpectateButton, upperRow.SpectateButton.isActiveAndEnabled);
            }
        }

        private void OnDestroy()
        {
            InputManager.OnBackPerformed -= HandleBackPerformed;
            
            previousButton.onClick.RemoveListener(HandlePreviousSelected);
            nextButton.onClick.RemoveListener(HandleNextSelected);
            globalButton.onClick.RemoveListener(HandleGlobalSelected);
            friendsButton.onClick.RemoveListener(HandleFriendsSelected);
            refreshButton.onClick.RemoveListener(HandleRefreshSelected);
            backButton.onClick.RemoveListener(HandleBackSelected);
            
            leaderboardCancellationTokenSource?.Cancel();
            leaderboardCancellationTokenSource?.Dispose();
            
            foreach (var row in leaderboardRows)
            {
                row.OnClickedPlay -= HandleClickedPlay;
                row.OnDownloadStarted -= HandleDownloadStarted;
                row.OnDownloadFinished -= HandleDownloadFinished;
            }
        }
    }
}
