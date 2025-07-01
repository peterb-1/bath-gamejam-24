using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Core.Saving
{
    [Serializable]
    public class PreferenceData
    {
        [SerializeField] 
        private List<PreferenceEntry> entries = new();

        private Dictionary<SettingId, CachedSetting> values = new();
        private Dictionary<SettingId, AbstractSettingBase> settings = new();

        public event Action<SettingId, object> OnSettingChanged;

        private bool isDirty;

        public void Initialise(AbstractSettingBase[] allSettings)
        {
            var savedValues = new Dictionary<SettingId, CachedSetting>();

            foreach (var entry in entries)
            {
                try
                {
                    Type settingType = null;
                    
                    foreach (var setting in allSettings)
                    {
                        if (entry.settingId != setting.SettingId) continue;
                        
                        settingType = setting.GetValueType();
                        break;
                    }
                    
                    if (settingType == null) continue;

                    var serializableValueType = typeof(SerializableValue<>).MakeGenericType(settingType);
                    var serializableValue = JsonUtility.FromJson(entry.valueJson, serializableValueType);
                    var fieldInfo = serializableValueType.GetField("value");

                    var value = fieldInfo.GetValue(serializableValue);
                    savedValues[entry.settingId] = new CachedSetting
                    {
                        Value = value,
                        Type = settingType,
                        FieldInfo = fieldInfo
                    };
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to load setting {entry.settingId}: {ex.Message}");
                }
            }

            foreach (var setting in allSettings)
            {
                settings[setting.SettingId] = setting;
                
                if (savedValues.TryGetValue(setting.SettingId, out var saved))
                {
                    values[setting.SettingId] = saved;
                }
                else
                {
                    var defaultValue = setting.GetDefaultValue();
                    var type = defaultValue.GetType();
                    var serializableValueType = typeof(SerializableValue<>).MakeGenericType(type);
                    var fieldInfo = serializableValueType.GetField("value");

                    values[setting.SettingId] = new CachedSetting
                    {
                        Value = defaultValue,
                        Type = type,
                        FieldInfo = fieldInfo
                    };

                    isDirty = true;
                }
            }
        }

        public bool TryGetSetting(SettingId settingId, out AbstractSettingBase setting)
        {
            return settings.TryGetValue(settingId, out setting);
        }

        public bool TryGetValue<T>(SettingId settingId, out T value)
        {
            if (values.TryGetValue(settingId, out var cached) && cached.Value is T typedValue)
            {
                value = typedValue;
                return true;
            }

            value = default;
            return false;
        }

        public void SetValue<T>(SettingId settingId, T value)
        {
            if (values.TryGetValue(settingId, out var cached))
            {
                cached.Value = value;
                values[settingId] = cached;
            }
            else
            {
                values[settingId] = new CachedSetting
                {
                    Value = value,
                    Type = typeof(T),
                    FieldInfo = typeof(SerializableValue<T>).GetField("value")
                };
            }
            
            OnSettingChanged?.Invoke(settingId, value);

            isDirty = true;
        }

        public void PrepareForSerialization()
        {
            if (!isDirty) return;
            
            entries.Clear();

            foreach (var (key, cached) in values)
            {
                var serializableValueType = typeof(SerializableValue<>).MakeGenericType(cached.Type);
                var serializableValue = Activator.CreateInstance(serializableValueType);
                
                cached.FieldInfo.SetValue(serializableValue, cached.Value);

                entries.Add(new PreferenceEntry
                {
                    settingId = key,
                    valueJson = JsonUtility.ToJson(serializableValue)
                });
            }

            isDirty = false;
        }

        [Serializable]
        public struct PreferenceEntry
        {
            public SettingId settingId;
            public string valueJson;
        }

        private struct CachedSetting
        {
            public object Value;
            public Type Type;
            public FieldInfo FieldInfo;
        }

        [Serializable]
        private class SerializableValue<T>
        {
            public T value;
        }
    }
}