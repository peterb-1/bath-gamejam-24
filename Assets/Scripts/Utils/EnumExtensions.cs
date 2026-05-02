using System;

namespace Utils
{
    public static class EnumExtensions
    {
        public static bool HasAnyFlag<T>(this T value, T flags) where T : Enum
        {
            var a = Convert.ToInt64(value);
            var b = Convert.ToInt64(flags);
            return (a & b) != 0;
        }
    }
}
