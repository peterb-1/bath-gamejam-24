using UnityEngine;

namespace Utils
{
    public static class Vector2Extensions
    {
        public static Vector2 SoftClamp(this Vector2 value, Vector2 min, Vector2 max, float damping)
        {
            var clampedX = SoftClampAxis(value.x, min.x, max.x, damping);
            var clampedY = SoftClampAxis(value.y, min.y, max.y, damping);

            return new Vector2(clampedX, clampedY);
        }

        private static float SoftClampAxis(float value, float min, float max, float damping)
        {
            if (value < min)
            {
                var diff = min - value;
                var dampedDiff = Mathf.Pow(diff, damping);
                return min - dampedDiff;
            }
            
            if (value > max)
            {
                var diff = value - max;
                var dampedDiff = Mathf.Pow(diff, damping);
                return max + dampedDiff;
            }

            return value;
        }
    }
}