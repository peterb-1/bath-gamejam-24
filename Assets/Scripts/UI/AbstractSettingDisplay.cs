using System;
using Core.Saving;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public abstract class AbstractSettingDisplay : MonoBehaviour
    {
        [field: SerializeField] 
        private SettingId settingId;
        
        [field: SerializeField] 
        private TMP_Text settingNameText;

        public event Action<AbstractSettingDisplay> OnHover;
        public event Action OnUnhover;

        protected AbstractSettingBase Setting { get; private set; }
        public string SettingName => Setting.DisplayName;
        public string SettingDescription => Setting.Description;

        protected virtual async void Awake()
        {
            await UniTask.WaitUntil(() => SaveManager.IsReady);

            var preferenceData = SaveManager.Instance.SaveData.PreferenceData;

            if (preferenceData.TryGetSetting(settingId, out var setting))
            {
                Setting = setting;
                settingNameText.text = SettingName;
            }
            
            if (preferenceData.TryGetValue(settingId, out object value))
            {
                SetDisplay(value);
            }
        }

        protected abstract void SetDisplay(object value);
        
        public abstract Selectable[] GetSelectables();

        protected void FireHoverEvent()
        {
            OnHover?.Invoke(this);
        }
        
        protected void FireUnhoverEvent()
        {
            OnUnhover?.Invoke();
        }
    }
}