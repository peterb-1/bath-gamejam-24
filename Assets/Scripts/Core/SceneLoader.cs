using System;
using System.Collections.Generic;
using Audio;
using Core.Saving;
using Cysharp.Threading.Tasks;
using Gameplay.Camera;
using Gameplay.Input;
using NaughtyAttributes;
using Steam;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] 
        private LoadingScreen loadingScreen;

        [SerializeField] 
        private SceneConfig levelSelectScene;

        [field: SerializeField, ReadOnly] 
        public List<SceneConfig> SceneConfigs { get; private set; }
        
        public static SceneLoader Instance { get; private set; }
        
        public SceneConfig CurrentSceneConfig { get; private set; }
        public SceneConfig PreviousSceneConfig { get; private set; }
        public CustomDataContainer SceneLoadContext { get; private set; }
        public LevelData CurrentLevelData { get; private set; }

        public bool IsLoading { get; private set; }
        
        public int CurrentDistrict => CurrentSceneConfig != null && CurrentSceneConfig.IsLevelScene
            ? CurrentSceneConfig.LevelConfig.DistrictNumber
            : 0;
        
        public int PreviousDistrict => PreviousSceneConfig != null && PreviousSceneConfig.IsLevelScene
            ? PreviousSceneConfig.LevelConfig.DistrictNumber
            : 0;

        public static event Action OnSceneLoadStart;
        public static event Action OnSceneLoaded;
        
        private async void Awake()
        {
            if (Instance != null && Instance != this)
            {
                // would usually log an error, but we expect this to happen when loading a new scene
                Destroy(gameObject);
                return;
            }

            Instance = this;
            transform.parent = null;
            DontDestroyOnLoad(this);

            foreach (var config in SceneConfigs)
            {
                if (config.ScenePath == SceneManager.GetActiveScene().path)
                {
                    CurrentSceneConfig = config;

                    await UniTask.WaitUntil(() => SaveManager.IsReady);
                    
                    SetLevelData();

                    break;
                }
            }

            PreviousSceneConfig = null;

            InputManager.OnRestartPerformed += HandleRestartPerformed;

            // convenience for editor - play music when starting from any scene
            if (CurrentSceneConfig != null)
            {
                await UniTask.WaitUntil(AudioManager.IsReady);
                
                if (CurrentSceneConfig.IsLevelScene)
                {
                    AudioManager.Instance.PlayMusic((MusicIdentifier) CurrentSceneConfig.LevelConfig.DistrictNumber);
                }
                else if (CurrentSceneConfig == levelSelectScene)
                {
                    AudioManager.Instance.PlayMusic(MusicIdentifier.MainMenu);
                }
            }
        }
        
        public static bool IsReady() => Instance != null;

        private void HandleRestartPerformed()
        {
            if (IsLoading) return;
            
            AudioManager.Instance.Play(AudioClipIdentifier.ButtonClick);
            
            GameLogger.Log("Quick-restarting current level...", this);
            
            ReloadCurrentScene();
        }
        
        public void ReloadCurrentScene()
        {
            LoadScene(CurrentSceneConfig, SceneLoadContext);
        }

        public void LoadLevelSelect(CustomDataContainer context = null)
        {
            LoadScene(levelSelectScene, context);
        }

        public void LoadScene(SceneConfig sceneConfig, CustomDataContainer context = null)
        {
            if (IsLoading) return;

            LoadSceneAsync(sceneConfig, context).Forget();
        }

        private async UniTask LoadSceneAsync(SceneConfig sceneConfig, CustomDataContainer context)
        {
            GameLogger.Log($"Loading scene {sceneConfig.name}...", this);
            
            IsLoading = true;
            SceneLoadContext = context;
            
            EventSystem.current.enabled = false;
            
            OnSceneLoadStart?.Invoke();

            // if we're done with the current scene, upload any high scores to Steam
            if (SteamLeaderboards.IsReady() && CurrentSceneConfig.IsLevelScene && sceneConfig != CurrentSceneConfig)
            {
                SteamLeaderboards.Instance.QueueScoreUpload(CurrentSceneConfig.LevelConfig);
            }

            await loadingScreen.ShowAsync();
            
            SaveManager.Instance.Save();
            
            PreviousSceneConfig = CurrentSceneConfig;
            CurrentSceneConfig = sceneConfig;

            SetLevelData();

            if (CurrentDistrict != PreviousDistrict)
            {
                AudioManager.Instance.PlayMusic((MusicIdentifier) CurrentDistrict);
            }
            
            await SceneManager.LoadSceneAsync(sceneConfig.ScenePath);

            await UniTask.WaitUntil(CameraAccessService.IsReady);
            
            if (sceneConfig.IsLevelScene)
            {
                CameraAccessService.Instance.PostProcessOverrider.SetPostProcessOverride(sceneConfig.LevelConfig.DistrictNumber);
            }
            else
            {
                CameraAccessService.Instance.PostProcessOverrider.RemovePostProcessOverride();
            }

            GameLogger.Log($"Loaded scene {sceneConfig.name} successfully!", this);
            
            EventSystem.current.enabled = true;
            
            OnSceneLoaded?.Invoke();
            
            await loadingScreen.HideAsync();
            
            IsLoading = false;
        }

        private void SetLevelData()
        {
            if (CurrentSceneConfig.IsLevelScene && 
                SaveManager.Instance != null && 
                SaveManager.Instance.SaveData.CampaignData.TryGetLevelData(CurrentSceneConfig.LevelConfig, out var levelData))
            {
                CurrentLevelData = levelData;
            }
            else
            {
                CurrentLevelData = null;
            }
        }
        
        private void OnDestroy()
        {
            if (Instance != this) return;
            Instance = null;
            
            InputManager.OnRestartPerformed -= HandleRestartPerformed;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            SceneConfigs.Clear();
            
            var guids = AssetDatabase.FindAssets($"t:{nameof(SceneConfig)}");
            
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<SceneConfig>(path);
                if (asset != null)
                {
                    SceneConfigs.Add(asset);
                }
            }
        }
#endif
    }
}
