using Audio;
using Core.Saving;
using Gameplay.Input;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UI
{
    public class QuitGamePopupBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private PageGroup popupPageGroup;
        
        [SerializeField] 
        private PageGroup levelSelectPageGroup;

        [SerializeField] 
        private LevelSelectUIBehaviour levelSelectUIBehaviour;

        [SerializeField] 
        private Button cancelButton;

        [SerializeField] 
        private Button quitButton;

        private void Awake()
        {
            InputManager.OnBackPerformed += HandleBackPerformed;
            
            cancelButton.onClick.AddListener(HidePopup);
            quitButton.onClick.AddListener(QuitGame);
        }
        
        private void HandleBackPerformed()
        {
            if (levelSelectPageGroup.IsInteractable)
            {
                ShowPopup();
            }
            else if (popupPageGroup.IsShowing)
            {
                HidePopup();
            }
        }
        
        private void ShowPopup()
        {
            levelSelectPageGroup.SetInteractable(false);
            popupPageGroup.ShowGroupImmediate();
        }
        
        private void HidePopup()
        {
            levelSelectPageGroup.SetInteractable(true);
            popupPageGroup.HideGroupImmediate();
            
            levelSelectUIBehaviour.SelectLastSelectedItem();
        }

        private void QuitGame()
        {
            SaveManager.Instance.Save();

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDestroy()
        {
            InputManager.OnBackPerformed -= HandleBackPerformed;
            
            cancelButton.onClick.RemoveListener(HidePopup);
            quitButton.onClick.RemoveListener(QuitGame);
        }
    }
}