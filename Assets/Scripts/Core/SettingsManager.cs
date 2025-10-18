using System;
using System.Collections.Generic;
using Core.Saving;
using UnityEngine;

namespace Core
{
    public class SettingsManager : MonoBehaviour
    {
        [SerializeField] 
        private AbstractSettingBase[] settings;
        
        private Dictionary<SettingId, AbstractSettingBase> settingLookup;
        
        public static SettingsManager Instance { get; private set; }
        
        private void Awake() 
        {
            if (Instance != null && Instance != this)
            {
                // would usually log an error, but we expect this to happen when loading a new scene
                Destroy(gameObject);
                return;
            }

            settingLookup = new Dictionary<SettingId, AbstractSettingBase>();
            
            foreach (var setting in settings) 
            {
                settingLookup[setting.SettingId] = setting;
            }
            
            Instance = this;
            transform.parent = null;
            DontDestroyOnLoad(this);
        }
    
        public T GetValue<T>(SettingId settingId) 
        {
            var setting = settingLookup[settingId] as AbstractSetting<T>;
            
            if (setting == null)
            {
                throw new InvalidOperationException($"Setting {settingId} is not of type {typeof(T)}");
            }

            if (SaveManager.Instance.SaveData.PreferenceData.TryGetValue(settingId, out T value))
            {
                return value;
            }
                
            return setting.DefaultValue;
        }
    
        public void SetValue<T>(SettingId settingId, T value) 
        {
            var setting = settingLookup[settingId] as AbstractSetting<T>;
            
            if (setting == null)
            {
                throw new InvalidOperationException($"Setting {settingId} is not of type {typeof(T)}");
            }

            SaveManager.Instance.SaveData.PreferenceData.SetValue(settingId, value);
        }
    }
}