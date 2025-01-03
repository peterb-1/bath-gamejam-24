using Audio;
using Core;
using Gameplay.Core;
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
            SceneLoader.Instance.ReloadCurrentScene();
        }
        
        private void HandleQuitClicked()
        {
            GameLogger.Log("Quitting current level from pause menu...", this);
            PauseManager.Instance.UnpauseInvisible();
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