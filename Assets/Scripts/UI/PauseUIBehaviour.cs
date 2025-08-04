using Audio;
using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Core;
using Gameplay.Ghosts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class PauseUIBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private TMP_Text levelInfoText;
        
        [SerializeField] 
        private TMP_Text spectatingInfoText;
        
        [SerializeField] 
        private TMP_Text retryText;

        [SerializeField] 
        private Button resumeButton;

        [SerializeField] 
        private Button retryButton;

        [SerializeField] 
        private Button quitButton;

        private void Awake()
        {
            resumeButton.onClick.AddListener(HandleResumeClicked);
            retryButton.onClick.AddListener(HandleRetryClicked);
            quitButton.onClick.AddListener(HandleQuitClicked);

            if (SceneLoader.Instance.SceneLoadContext != null && 
                SceneLoader.Instance.SceneLoadContext.TryGetCustomData(SpectateVictoryUIBehaviour.LEADERBOARD_NAME_KEY, out string leaderboardName) &&
                SceneLoader.Instance.SceneLoadContext.TryGetCustomData(GhostRunner.SPECTATE_KEY, out bool isSpectating) &&
                isSpectating)
            {
                retryText.text = "REWATCH";
                spectatingInfoText.text = $"Spectating {leaderboardName}";
            }
            else
            {
                spectatingInfoText.gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            SetLevelInfoText();
        }

        private void SetLevelInfoText()
        {
            var sceneConfig = SceneLoader.Instance.CurrentSceneConfig;

            if (sceneConfig.IsLevelScene)
            {
                levelInfoText.text = sceneConfig.LevelConfig.GetLevelText();
            }
            else
            {
                GameLogger.LogWarning("Could not obtain current level config for level info text.", this);
                levelInfoText.text = "MISSING LEVEL CONFIG";
            }
        }

        private void HandleResumeClicked()
        {
            PauseManager.Instance.Unpause();
        }

        private void HandleRetryClicked()
        {
            GameLogger.Log("Restarting current level from pause menu...", this);
            PauseManager.Instance.UnpauseInvisible();
            AudioManager.Instance.ClearPausedSounds();
            
            Time.timeScale = 1f;
            
            SceneLoader.Instance.ReloadCurrentScene();
        }
        
        private void HandleQuitClicked()
        {
            GameLogger.Log("Quitting current level from pause menu...", this);
            PauseManager.Instance.UnpauseInvisible();
            AudioManager.Instance.ClearPausedSounds();

            Time.timeScale = 1f;
            
            SceneLoader.Instance.LoadLevelSelect();
        }

        private void OnDestroy()
        {
            resumeButton.onClick.RemoveListener(HandleResumeClicked);
            retryButton.onClick.RemoveListener(HandleRetryClicked);
            quitButton.onClick.RemoveListener(HandleQuitClicked);
        }
    }
}