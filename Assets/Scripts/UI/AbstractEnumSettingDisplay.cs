using Core.Saving;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public abstract class AbstractEnumSettingDisplay : AbstractSettingDisplay
    {
        [field: SerializeField] 
        protected SettingCarouselBehaviour Carousel { get; private set; }

        protected override void Awake()
        {
            SetupCarousel();

            Carousel.OnValueChanged += HandleValueChanged;
            Carousel.OnHover += FireHoverEvent;
            Carousel.OnUnhover += FireUnhoverEvent;
            
            base.Awake();
        }

        protected abstract void SetupCarousel();

        protected override void SetDisplay(object value)
        {
            Carousel.SetCurrent(value);
        }

        public override Selectable[] GetSelectables()
        {
            return Carousel.Selectables;
        }

        private void HandleValueChanged(object newValue)
        {
            SaveManager.Instance.SaveData.PreferenceData.SetValue(Setting.SettingId, newValue);
        }

        private void OnDestroy()
        {
            Carousel.OnValueChanged -= HandleValueChanged;
            Carousel.OnHover -= FireHoverEvent;
            Carousel.OnUnhover -= FireUnhoverEvent;
        }
    }
}