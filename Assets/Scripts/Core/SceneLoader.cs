using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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

        [SerializeField, ReadOnly] 
        private List<SceneConfig> sceneConfigs;
        
        public static SceneLoader Instance { get; private set; }

        public static event Action OnSceneLoadStart;
        public static event Action OnSceneLoaded;
        
        public SceneConfig CurrentSceneConfig { get; private set; }

        private bool isLoading;
        
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

            foreach (var config in sceneConfigs)
            {
                if (config.ScenePath == SceneManager.GetActiveScene().path)
                {
                    CurrentSceneConfig = config;
                }
            }
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
            if (isLoading) return;

            LoadSceneAsync(sceneConfig).Forget();
        }

        private async UniTask LoadSceneAsync(SceneConfig sceneConfig)
        {
            GameLogger.Log($"Loading scene {sceneConfig.name}...", this);
            
            isLoading = true;
            
            OnSceneLoadStart?.Invoke();

            await loadingScreen.ShowAsync();
            
            CurrentSceneConfig = sceneConfig;
            
            await SceneManager.LoadSceneAsync(sceneConfig.ScenePath);

            GameLogger.Log($"Loaded scene {sceneConfig.name} successfully!", this);
            
            OnSceneLoaded?.Invoke();
            
            await loadingScreen.HideAsync();
            
            isLoading = false;
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            sceneConfigs.Clear();
            
            var guids = AssetDatabase.FindAssets($"t:{nameof(SceneConfig)}");
            
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<SceneConfig>(path);
                if (asset != null)
                {
                    sceneConfigs.Add(asset);
                }
            }
        }
#endif
    }
}
