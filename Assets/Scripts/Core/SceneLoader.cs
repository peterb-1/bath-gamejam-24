using System;
using System.Collections.Generic;
using Audio;
using Cysharp.Threading.Tasks;
using Gameplay.Input;
using NaughtyAttributes;
using UnityEngine;
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

        public bool IsLoading { get; private set; }
        
        public static event Action OnSceneLoadStart;
        public static event Action OnSceneLoaded;
        
        private void Awake()
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
                }
            }

            PreviousSceneConfig = null;

            InputManager.OnRestartPerformed += HandleRestartPerformed;
        }
        
        public static bool IsReady() => Instance != null;

        private void HandleRestartPerformed()
        {
            if (IsLoading) return;
            
            AudioManager.Instance.Play(AudioClipIdentifier.ButtonClick);
            
            ReloadCurrentScene();
        }
        
        public void ReloadCurrentScene()
        {
            LoadScene(CurrentSceneConfig);
        }

        public void LoadLevelSelect()
        {
            LoadScene(levelSelectScene);
        }

        public void LoadScene(SceneConfig sceneConfig)
        {
            if (IsLoading) return;

            LoadSceneAsync(sceneConfig).Forget();
        }

        private async UniTask LoadSceneAsync(SceneConfig sceneConfig)
        {
            GameLogger.Log($"Loading scene {sceneConfig.name}...", this);
            
            IsLoading = true;
            
            OnSceneLoadStart?.Invoke();

            await loadingScreen.ShowAsync();
            
            PreviousSceneConfig = CurrentSceneConfig;
            CurrentSceneConfig = sceneConfig;
            
            await SceneManager.LoadSceneAsync(sceneConfig.ScenePath);

            GameLogger.Log($"Loaded scene {sceneConfig.name} successfully!", this);
            
            OnSceneLoaded?.Invoke();
            
            await loadingScreen.HideAsync();
            
            IsLoading = false;
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
