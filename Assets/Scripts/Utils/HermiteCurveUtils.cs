using UnityEngine;

namespace Utils
{
    public static class HermiteCurveUtils
    {
        public static Vector3 CubicHermite(Vector3 p0, Vector3 m0, Vector3 p1, Vector3 m1, float t)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            
            var h00 = 2f * t3 - 3f * t2 + 1f;
            var h10 = t3 - 2f * t2 + t;
            var h01 = -2f * t3 + 3f * t2;
            var h11 = t3 - t2;
            
            return h00 * p0 + h10 * m0 + h01 * p1 + h11 * m1;
        }
    }
}