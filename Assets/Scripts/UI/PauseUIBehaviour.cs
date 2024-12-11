using Core;
using Gameplay.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PauseUIBehaviour : MonoBehaviour
    {
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

        private void HandleResumeClicked()
        {
            PauseManager.Instance.Unpause();
        }

        private void HandleRetryClicked()
        {
            PauseManager.Instance.UnpauseInvisible();
            SceneLoader.Instance.ReloadCurrentScene();
        }
        
        private void HandleQuitClicked()
        {
            throw new System.NotImplementedException();
        }

        private void OnDestroy()
        {
            resumeButton.onClick.RemoveListener(HandleResumeClicked);
            retryButton.onClick.RemoveListener(HandleRetryClicked);
            quitButton.onClick.RemoveListener(HandleQuitClicked);
        }
    }
}