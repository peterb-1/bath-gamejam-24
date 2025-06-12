using UnityEngine;

namespace Utils
{
    public static class TimeUtils
    {
        public static int ToMilliseconds(this float seconds)
        {
            return Mathf.RoundToInt(seconds * 1000f);
        }
        
        public static float ToSeconds(this int milliseconds)
        {
            return milliseconds / 1000f;
        }
    }
}