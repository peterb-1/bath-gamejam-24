using System;

namespace Core.Saving
{
    public class EnumSetting<TEnum> : AbstractSetting<TEnum> where TEnum : Enum {}
}