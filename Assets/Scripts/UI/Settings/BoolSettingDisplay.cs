using Core.Saving;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Settings
{
    public class BoolSettingDisplay : AbstractSettingDisplay
    {
        [SerializeField] 
        private ExtendedToggle toggle;

        protected override void Awake()
        {
            base.Awake();

            toggle.onValueChanged.AddListener(HandleValueChanged);
            
            toggle.OnHover += HandleHover;
            toggle.OnUnhover += HandleUnhover;
        }

        protected override void SetDisplay(object value)
        {
            if (value is bool b)
            {
                toggle.SetIsOnWithoutNotify(b);
            }
        }

        public override Selectable[] GetSelectables()
        {
            return new Selectable[] { toggle };
        }

        private void HandleValueChanged(bool value)
        {
            SaveManager.Instance.SaveData.PreferenceData.SetValue(Setting.SettingId, value);
        }

        private void HandleHover(ExtendedToggle _)
        {
            FireHoverEvent();
        }
        
        private void HandleUnhover(ExtendedToggle _)
        {
            FireUnhoverEvent();
        }

        private void OnDestroy()
        {
            toggle.onValueChanged.RemoveListener(HandleValueChanged);
            
            toggle.OnHover -= HandleHover;
            toggle.OnUnhover -= HandleUnhover;
        }
    }
}