using UnityEngine;

namespace Utils
{
    public static class AngleUtils
    {
        public const float FULL_CIRCLE = 360f;
        
        public static float AngleLerp(float a, float b, float t)
        {
            a = MathsUtils.Modulo(a, FULL_CIRCLE);
            b = MathsUtils.Modulo(b, FULL_CIRCLE);

            var delta = Mathf.DeltaAngle(a, b);
            var result = a + delta * t;

            return MathsUtils.Modulo(result, FULL_CIRCLE);
        }
    }
}