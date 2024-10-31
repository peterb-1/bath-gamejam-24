using UnityEngine;

namespace Utils
{
    public static class ArrayUtils
    {
        public static T RandomChoice<T>(T[] array)
        {
            if (array.Length == 0)
            {
                GameLogger.LogError("Cannot get random element from array with zero length!");
                return default;
            }
            
            return array[Random.Range(0, array.Length)];
        }
    }
}
