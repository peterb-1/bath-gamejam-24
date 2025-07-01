using Core.Saving;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class FloatSettingDisplay : AbstractSettingDisplay
    {
        [SerializeField]
        private ExtendedSlider slider;

        protected override void Awake()
        {
            slider.onValueChanged.AddListener(HandleValueChanged);
            
            slider.OnHover += HandleHover;
            slider.OnUnhover += HandleUnhover;
            
            base.Awake();
        }

        protected override void SetDisplay(object value)
        {
            if (value is float f)
            {
                slider.SetValueWithoutNotify(f);
            }
        }

        public override Selectable GetSelectable()
        {
            return slider;
        }

        private void HandleValueChanged(float t)
        {
            if (Setting is FloatSetting floatSetting)
            {
                var value = Mathf.Lerp(floatSetting.Min, floatSetting.Max, t);
                
                SaveManager.Instance.SaveData.PreferenceData.SetValue(Setting.SettingId, value);
            }
        }

        private void HandleHover(ExtendedSlider _)
        {
            FireHoverEvent();
        }
        
        private void HandleUnhover(ExtendedSlider _)
        {
            FireUnhoverEvent();
        }

        private void OnDestroy()
        {
            slider.onValueChanged.RemoveListener(HandleValueChanged);
            
            slider.OnHover -= HandleHover;
            slider.OnUnhover -= HandleUnhover;
        }
    }
}