using Core.Saving;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Settings
{
    public class FloatSettingDisplay : AbstractSettingDisplay
    {
        [SerializeField]
        private ExtendedSlider slider;

        protected override async void Awake()
        {
            base.Awake();

            slider.onValueChanged.AddListener(HandleValueChanged);
            
            slider.OnHover += HandleHover;
            slider.OnUnhover += HandleUnhover;
            
            // can't await base method for some reason
            await UniTask.WaitUntil(() => Setting != null);

            if (Setting is FloatSetting floatSetting)
            {
                slider.minValue = floatSetting.Min;
                slider.maxValue = floatSetting.Max;
            }
        }

        protected override void SetDisplay(object value)
        {
            if (value is float f)
            {
                slider.SetValueWithoutNotify(f);
            }
        }

        public override Selectable[] GetSelectables()
        {
            return new Selectable[] { slider };
        }

        private void HandleValueChanged(float value)
        {
            SaveManager.Instance.SaveData.PreferenceData.SetValue(Setting.SettingId, value);
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