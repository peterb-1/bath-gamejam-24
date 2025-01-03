using Audio;
using Core;
using Gameplay.Input;
using UI;
using UnityEngine;
using Utils;

namespace Gameplay.Core
{
    public class PauseManager : MonoBehaviour
    {
        [SerializeField] 
        private PageGroup pauseMenuPageGroup;

        [SerializeField] 
        private PageGroup gameplayPageGroup;
        
        public bool IsPaused { get; private set; }

        public static PauseManager Instance { get; private set; }
        
        private bool wasPausedBeforeLostFocus;
        private float prePauseTimeScale;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                GameLogger.LogError("Cannot have more than one PauseManager in the scene at once! Destroying this one.");
                Destroy(this);
                return;
            }

            Instance = this;
            
            IsPaused = false;
            
            // due to a weird quirk where OnApplicationFocus gets called with hasFocus = true on startup
            wasPausedBeforeLostFocus = true;
            
            InputManager.OnPauseToggled += HandlePauseToggled;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            // ensure that we are always paused when losing focus during gameplay
            // but remember which state we were in to restore it when focus is regained

            if (!InputManager.AreInputsEnabled) return;
            
            GameLogger.Log($"Acting on application focus callback with value {hasFocus}...", this);
            
            if (!hasFocus)
            {
                wasPausedBeforeLostFocus = IsPaused;
                
                if (!IsPaused)
                {
                    TogglePauseInvisible();
                }
            }
            else if (!wasPausedBeforeLostFocus)
            {
                TogglePauseInvisible();
            }
        }

        private void HandlePauseToggled()
        {
            TogglePause();

            if (IsPaused)
            {
                AudioManager.Instance.Play(AudioClipIdentifier.ButtonClick);
            }
        }

        // toggle pause for losing focus without bringing up the menu
        private void TogglePauseInvisible()
        {
            if (SceneLoader.Instance != null && SceneLoader.Instance.IsLoading) return;
            
            IsPaused = !IsPaused;
            
            GameLogger.Log($"Invisibly setting paused state to {IsPaused}!", this);
            
            if (IsPaused)
            {
                prePauseTimeScale = Time.timeScale;
                
                Time.timeScale = 0.0f;
            }
            else
            {
                Time.timeScale = prePauseTimeScale;
            }
        }

        // full pause toggle for standard purposes
        private void TogglePause()
        {
            if (SceneLoader.Instance != null && SceneLoader.Instance.IsLoading) return;
            
            IsPaused = !IsPaused;
            
            GameLogger.Log($"Setting paused state to {IsPaused}!", this);

            if (IsPaused)
            {
                pauseMenuPageGroup.ShowGroupImmediate();
                gameplayPageGroup.HideGroupImmediate();
                
                prePauseTimeScale = Time.timeScale;
                
                Time.timeScale = 0.0f;
            }
            else
            {
                pauseMenuPageGroup.HideGroupImmediate();
                gameplayPageGroup.ShowGroupImmediate();
                
                Time.timeScale = prePauseTimeScale;
            }
        }
        
        public void UnpauseInvisible()
        {
            if (IsPaused)
            {
                TogglePauseInvisible();
            }
        }

        public void Unpause()
        {
            if (IsPaused)
            {
                TogglePause();
            }
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            Instance = null;
            
            InputManager.OnPauseToggled -= HandlePauseToggled;
        }
    }
}