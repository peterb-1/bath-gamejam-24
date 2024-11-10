using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Core
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] 
        private LoadingScreen loadingScreen;
        
        public static SceneLoader Instance { get; private set; }

        public static event Action OnSceneLoadStart;
        public static event Action OnSceneLoaded;

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
        }

        public void LoadScene(Scene scene)
        {
            if (isLoading) return;

            LoadSceneAsync(scene).Forget();
        }

        public void ReloadCurrentScene()
        {
            LoadScene(SceneManager.GetActiveScene());
        }

        private async UniTask LoadSceneAsync(Scene scene)
        {
            GameLogger.Log($"Loading scene {scene.name}...", this);
            
            isLoading = true;
            
            OnSceneLoadStart?.Invoke();

            await loadingScreen.ShowAsync();
            
            await SceneManager.LoadSceneAsync(scene.name);
            
            GameLogger.Log("Loaded scene successfully!", this);
            
            OnSceneLoaded?.Invoke();
            
            await loadingScreen.HideAsync();
            
            isLoading = false;
        }
    }
}
