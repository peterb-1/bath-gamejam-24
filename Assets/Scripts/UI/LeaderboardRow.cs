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
        private float ghostTime;
        private bool rowBelongsToCurrentUser;

        public Button GhostButton => ghostButton;

        public event Action OnDownloadStarted;
        public event Action OnDownloadFinished;

        private void Awake()
        {
            ghostButton.onClick.AddListener(HandleGhostButtonClicked);
        }

        private void HandleGhostButtonClicked()
        {
            if (rowBelongsToCurrentUser || downloadedGhostData != null)
            {
                LoadSceneWithGhostData();
            }
            else
            {
                GetGhostDataAsync().Forget();
            }
        }

        private async UniTask GetGhostDataAsync()
        {
            OnDownloadStarted?.Invoke();
            
            downloadSpinner.gameObject.SetActive(true);
            downloadSpinner.StartSpinner();

            ghostButtonImage.enabled = false;

            downloadedGhostData = await SteamLeaderboards.Instance.TryGetGhostDataAsync(currentSceneConfig.LevelConfig, ghostFileId);

            downloadSpinner.StopSpinner();
            downloadSpinner.gameObject.SetActive(false);

            ghostButtonImage.enabled = true;
            ghostButtonImage.sprite = downloadedGhostData == null ? downloadSprite : playSprite;

            if (InputManager.CurrentControlScheme is not ControlScheme.Mouse && EventSystem.current.currentSelectedGameObject == null)
            {
                ghostButton.Select();
            }
            
            OnDownloadFinished?.Invoke();
        }

        private void LoadSceneWithGhostData()
        {
            var sceneLoadContext = new CustomDataContainer();
            var ghostContext = new GhostContext(downloadedGhostData, ghostTime);
            
            sceneLoadContext.SetCustomData(GhostRunner.GHOST_DATA_KEY, ghostContext);
            
            SceneLoader.Instance.LoadScene(currentSceneConfig, sceneLoadContext);
        }

        public void SetDetails(int position, CSteamID steamID, float time, ulong fileId, SceneConfig sceneConfig)
        {
            rowBelongsToCurrentUser = SteamUser.GetSteamID().m_SteamID == steamID.m_SteamID;
            currentPlayerBackground.enabled = rowBelongsToCurrentUser;
            ghostTime = time;
            
            positionText.text = $"{position}";
            usernameText.text = steamID.GetUsername();
            timeText.text = TimerBehaviour.GetFormattedTime(time);
            rankingStarUIBehaviour.SetRanking(sceneConfig.LevelConfig.GetTimeRanking(time));
            downloadSpinner.gameObject.SetActive(false);

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
        }
        
        public void EnableDownloads()
        {
            ghostButton.interactable = true;
        }
        
        public void DisableDownloads()
        {
            ghostButton.interactable = false;
        }

        private void OnDestroy()
        {
            ghostButton.onClick.RemoveListener(HandleGhostButtonClicked);
        }
    }
}