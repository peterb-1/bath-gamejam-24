using NaughtyAttributes;
using UnityEngine;
using Utils;

namespace Core.Saving
{
    public class SaveManager : MonoBehaviour
    {
        private const string SAVE_PATH = "data.json";
        
        [SerializeField] 
        private AbstractSettingBase[] settings;
        
        public SaveData SaveData { get; private set; }
        
        public static bool IsReady { get; private set; }
        
        public static SaveManager Instance { get; private set; }

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

            SaveData = SaveUtils.Load<SaveData>(SAVE_PATH);
            
            await SaveData.InitialiseAsync(settings);

            IsReady = true;
        }

        public void Save()
        {
            SaveData.PreferenceData.PrepareForSerialization();
            
            SaveUtils.Save(SaveData, SAVE_PATH);
        }
        
#if UNITY_EDITOR
        [Button("[DEBUG] Unlock All Levels")]
        private void UnlockAllLevels()
        {
            foreach (var sceneConfig in SceneLoader.Instance.SceneConfigs)
            {
                if (sceneConfig.IsLevelScene
                    && SaveData.CampaignData.TryGetLevelData(sceneConfig.LevelConfig, out var levelData))
                {
                    levelData.TryUnlock();
                }
            }
            
            Save();
            
            SceneLoader.Instance.ReloadCurrentScene();
        }
#endif
    }
}