using System;

namespace UI.Settings
{
    public class EnumSettingDisplay<TEnum> : AbstractEnumSettingDisplay where TEnum : Enum
    {
        protected override void SetupCarousel()
        {
            var values = Enum.GetValues(typeof(TEnum));
            Carousel.SetOptions(values);
        }
    }
}