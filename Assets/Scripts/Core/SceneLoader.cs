using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] 
        private LoadingScreen loadingScreen;
        
        public static SceneLoader Instance { get; private set; }

        private bool isLoading;
        
        private void Awake()
        {
            if (Instance != null)
            {
                // would usually log an error, but we expect this to happen when loading a new scene
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this);
        }

        public void LoadScene(Scene scene)
        {
            if (isLoading) return;

            LoadSceneAsync(scene).Forget();
        }

        private async UniTask LoadSceneAsync(Scene scene)
        {
            isLoading = true;

            await loadingScreen.ShowAsync();
            
            await SceneManager.LoadSceneAsync(scene.name);
            
            await loadingScreen.HideAsync();
            
            isLoading = false;
        }
    }
}
