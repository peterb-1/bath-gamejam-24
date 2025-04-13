using UnityEngine;

namespace Utils
{
    public static class ColourUtils
    {
        public static Gradient WithTint(this Gradient gradient, Color tint)
        {
            var newGradient = new Gradient();

            var colorKeys = gradient.colorKeys;
            var alphaKeys = gradient.alphaKeys;

            for (var i = 0; i < colorKeys.Length; i++)
            {
                colorKeys[i].color *= tint;
            }

            for (var i = 0; i < alphaKeys.Length; i++)
            {
                alphaKeys[i].alpha *= tint.a;
            }

            newGradient.SetKeys(colorKeys, alphaKeys);
            
            return newGradient;
        }
    }
}