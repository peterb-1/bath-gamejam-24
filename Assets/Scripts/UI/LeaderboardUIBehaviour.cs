using System;
using System.Collections.Generic;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Core;
using Steam;
using TMPro;
using UnityEngine;
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
        private Button backButton;

        [SerializeField] 
        private LeaderboardRow[] leaderboardRows;

        private CancellationTokenSource leaderboardCancellationTokenSource;
        private List<SceneConfig> orderedSceneConfigs;
        private Action settingsClosedCallback;
        private int currentConfigIndex;

        private void Awake()
        {
            previousButton.onClick.AddListener(HandlePreviousSelected);
            nextButton.onClick.AddListener(HandleNextSelected);
            backButton.onClick.AddListener(HandleBackSelected);

            foreach (var row in leaderboardRows)
            {
                row.OnDownloadStarted += HandleDownloadStarted;
                row.OnDownloadFinished += HandleDownloadFinished;
            }
        }
        
        private void Update()
        {
            if (!SteamLeaderboards.IsReady()) return;
            
            var queueSize = SteamLeaderboards.Instance.UploadsQueued;

            uploadInfoText.text = queueSize switch
            {
                0 => "All scores uploaded!",
                1 => "1 score pending upload...",
                > 1 => $"{queueSize} scores pending upload...",
                _ => ""
            };
        }

        public void SetLevelConfigs(List<SceneConfig> sceneConfigs)
        {
            orderedSceneConfigs = sceneConfigs;
        }

        public void OpenLeaderboard(LevelConfig levelConfig, Action onClosedCallback)
        {
            for (var i = 0; i < orderedSceneConfigs.Count; i++)
            {
                if (orderedSceneConfigs[i].LevelConfig.Guid == levelConfig.Guid)
                {
                    currentConfigIndex = i;
                }
            }

            pageGroup.SetDefaultPage();
            pageGroup.ShowGroup(isForward: false);

            settingsClosedCallback = onClosedCallback;

            PopulateLeaderboard();
        }

        private void PopulateLeaderboard()
        {
            leaderboardCancellationTokenSource?.Cancel();
            leaderboardCancellationTokenSource?.Dispose();
            leaderboardCancellationTokenSource = new CancellationTokenSource();
            
            PopulateLeaderboardAsync(leaderboardCancellationTokenSource.Token).Forget();
        }

        private async UniTask PopulateLeaderboardAsync(CancellationToken token)
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

            var (wasCancelled, (result, entries)) = await SteamLeaderboards.Instance.TryGetGlobalScoresAsync(levelConfig, leaderboardRows.Length)
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
                row.SetDetails(entry.m_nGlobalRank, entry.m_steamIDUser, entry.m_nScore.ToSeconds(), fileId, sceneConfig);
            }
        }

        private void HandlePreviousSelected()
        {
            currentConfigIndex = MathsUtils.Modulo(currentConfigIndex - 1, orderedSceneConfigs.Count);
            PopulateLeaderboard();
        }

        private void HandleNextSelected()
        {
            currentConfigIndex = MathsUtils.Modulo(currentConfigIndex + 1, orderedSceneConfigs.Count);
            PopulateLeaderboard();
        }

        private void HandleBackSelected()
        {
            leaderboardCancellationTokenSource?.Cancel();
            pageGroup.HideGroup(isForward: true);
            settingsClosedCallback?.Invoke();
        }
        
        private void HandleDownloadStarted()
        {
            previousButton.interactable = false;
            nextButton.interactable = false;

            foreach (var row in leaderboardRows)
            {
                row.DisableDownloads();
            }
        }
        
        private void HandleDownloadFinished()
        {
            previousButton.interactable = true;
            nextButton.interactable = true;
            
            foreach (var row in leaderboardRows)
            {
                row.EnableDownloads();
            }
        }

        private void OnDestroy()
        {
            previousButton.onClick.RemoveListener(HandlePreviousSelected);
            nextButton.onClick.RemoveListener(HandleNextSelected);
            backButton.onClick.RemoveListener(HandleBackSelected);
            
            leaderboardCancellationTokenSource?.Cancel();
            leaderboardCancellationTokenSource?.Dispose();
            
            foreach (var row in leaderboardRows)
            {
                row.OnDownloadStarted -= HandleDownloadStarted;
                row.OnDownloadFinished -= HandleDownloadFinished;
            }
        }
    }
}
