using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public static class ArrayUtils
    {
        public static T RandomChoice<T>(this T[] array)
        {
            if (array.Length == 0)
            {
                GameLogger.LogError("Cannot get random element from array with zero length!");
                return default;
            }
            
            return array[Random.Range(0, array.Length)];
        }
        
        public static T RandomChoice<T>(this List<T> list)
        {
            if (list.Count == 0)
            {
                GameLogger.LogError("Cannot get random element from list with zero length!");
                return default;
            }
            
            return list[Random.Range(0, list.Count)];
        }

        public static int[] GetRandomIndices(int count)
        {
            var shuffledIndices = new int[count];
            
            for (var i = 0; i < count; i++)
            {
                shuffledIndices[i] = i;
            }

            // O(n) shuffle
            for (var i = count - 1; i > 0; i--)
            {
                var randomIndex = Random.Range(0, i + 1);
                (shuffledIndices[i], shuffledIndices[randomIndex]) = (shuffledIndices[randomIndex], shuffledIndices[i]);
            }

            return shuffledIndices;
        }
    }
}
