using System;
using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Ghosts;
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
    public class LeaderboardRow : MonoBehaviour
    {
        [SerializeField] 
        private TMP_Text positionText;
        
        [SerializeField] 
        private TMP_Text usernameText;
        
        [SerializeField] 
        private TMP_Text timeText;
        
        [SerializeField] 
        private RankingStarUIBehaviour rankingStarUIBehaviour;

        [SerializeField] 
        private Button ghostButton;
        
        [SerializeField] 
        private Button spectateButton;
        
        [SerializeField] 
        private GameObject spectatePadding;

        [SerializeField] 
        private Image currentPlayerBackground;

        [SerializeField] 
        private Image ghostButtonImage;

        [SerializeField] 
        private Sprite downloadSprite;
        
        [SerializeField] 
        private Sprite playSprite;

        [SerializeField] 
        private LoadingSpinner downloadSpinner;

        private SceneConfig currentSceneConfig;
        private GhostRun downloadedGhostData;
        private ulong ghostFileId;
        private int ghostMilliseconds;
        private bool rowBelongsToCurrentUser;

        public Button GhostButton => ghostButton;
        public Button SpectateButton => spectateButton;

        public event Action OnClickedPlay;
        public event Action OnDownloadStarted;
        public event Action OnDownloadFinished;

        private void Awake()
        {
            ghostButton.onClick.AddListener(HandleGhostButtonClicked);
            spectateButton.onClick.AddListener(HandleSpectateButtonClicked);
        }

        private void HandleGhostButtonClicked()
        {
            if (rowBelongsToCurrentUser || downloadedGhostData != null)
            {
                LoadSceneWithGhostData(false);
            }
            else
            {
                GetGhostDataAsync().Forget();
            }
        }
        
        private void HandleSpectateButtonClicked()
        {
            LoadSceneWithGhostData(true);
        }

        private async UniTask GetGhostDataAsync()
        {
            OnDownloadStarted?.Invoke();

            var fileToDownload = ghostFileId;
            
            downloadSpinner.gameObject.SetActive(true);
            downloadSpinner.StartSpinner();

            ghostButtonImage.enabled = false;

            downloadedGhostData = await SteamLeaderboards.Instance.TryGetGhostDataAsync(currentSceneConfig.LevelConfig, fileToDownload);

            downloadSpinner.StopSpinner();
            downloadSpinner.gameObject.SetActive(false);

            // might have left mid-download, and come back on a different level - if so, don't bother with the cleanup
            if (fileToDownload == ghostFileId)
            {
                ghostButtonImage.enabled = true;
                ghostButtonImage.sprite = downloadedGhostData == null ? downloadSprite : playSprite;
                
                spectateButton.gameObject.SetActive(true);
                spectatePadding.SetActive(false);

                if (InputManager.CurrentControlScheme is not ControlScheme.Mouse && EventSystem.current.currentSelectedGameObject == null)
                {
                    ghostButton.Select();
                }
            }
            
            OnDownloadFinished?.Invoke();
        }

        private void LoadSceneWithGhostData(bool isSpectating)
        {
            OnClickedPlay?.Invoke();
            
            var sceneLoadContext = new CustomDataContainer();
            var ghostContext = new GhostContext
            {
                GhostRun = downloadedGhostData, 
                DisplayMilliseconds = ghostMilliseconds
            };
            
            sceneLoadContext.SetCustomData(GhostRunner.GHOST_DATA_KEY, ghostContext);
            sceneLoadContext.SetCustomData(GhostRunner.LOAD_FROM_LEADERBOARD_KEY, true);
            sceneLoadContext.SetCustomData(GhostRunner.SPECTATE_KEY, isSpectating);
            sceneLoadContext.SetCustomData(SpectateVictoryUIBehaviour.LEADERBOARD_NAME_KEY, usernameText.text);
            
            SceneLoader.Instance.LoadScene(currentSceneConfig, sceneLoadContext);
        }

        public void SetDetails(int position, CSteamID steamID, int milliseconds, ulong fileId, SceneConfig sceneConfig)
        {
            rowBelongsToCurrentUser = SteamUser.GetSteamID().m_SteamID == steamID.m_SteamID;
            currentPlayerBackground.enabled = rowBelongsToCurrentUser;
            downloadedGhostData = null;
            ghostMilliseconds = milliseconds;
            
            positionText.text = $"{position}";
            usernameText.text = steamID.GetUsername();
            timeText.text = TimerBehaviour.GetFormattedTime(milliseconds);
            rankingStarUIBehaviour.SetRanking(sceneConfig.LevelConfig.GetTimeRanking(milliseconds));

            // might go back mid-download, then come back - it will still be downloading!
            if (fileId != ghostFileId)
            {
                downloadSpinner.gameObject.SetActive(false);
                ghostButtonImage.enabled = true;
            }

            ghostFileId = fileId;
            currentSceneConfig = sceneConfig;

            if (rowBelongsToCurrentUser)
            {
                ghostButtonImage.sprite = playSprite;
            }
            else
            {
                downloadedGhostData = SteamLeaderboards.Instance.TryGetOfflineGhostData(sceneConfig.LevelConfig, fileId);
                ghostButtonImage.sprite = downloadedGhostData != null ? playSprite : downloadSprite;
            }

            var isSpectateAllowed = rowBelongsToCurrentUser || downloadedGhostData != null;
            
            spectateButton.gameObject.SetActive(isSpectateAllowed);
            spectatePadding.SetActive(!isSpectateAllowed);
        }
        
        public void EnableDownloads()
        {
            ghostButton.interactable = true;
            spectateButton.interactable = true;
        }
        
        public void DisableDownloads()
        {
            ghostButton.interactable = false;
            spectateButton.interactable = false;
        }
        
        public void SetDownNavigation(Selectable left, Selectable right, bool shouldUseRight)
        {
            var ghostNavigation = ghostButton.navigation;
            var spectateNavigation = spectateButton.navigation;

            ghostNavigation.selectOnDown = left;
            spectateNavigation.selectOnDown = shouldUseRight ? right : left;

            ghostButton.navigation = ghostNavigation;
            spectateButton.navigation = spectateNavigation;
        }
        
        public void SetUpNavigation(Selectable left, Selectable right, bool shouldUseRight)
        {
            var ghostNavigation = ghostButton.navigation;
            var spectateNavigation = spectateButton.navigation;

            ghostNavigation.selectOnUp = left;
            spectateNavigation.selectOnUp = shouldUseRight ? right : left;

            ghostButton.navigation = ghostNavigation;
            spectateButton.navigation = spectateNavigation;
        }

        private void OnDestroy()
        {
            ghostButton.onClick.RemoveListener(HandleGhostButtonClicked);
            spectateButton.onClick.RemoveListener(HandleSpectateButtonClicked);
        }
    }
}