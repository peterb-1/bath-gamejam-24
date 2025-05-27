using UnityEngine;

namespace Utils
{
    public static class GameLogger
    {
        public static void Log(object message, Object context = null)
        {
            if (Debug.isDebugBuild)
            {
                Debug.Log($"[GAME] {message}", context);
            }
        }
        
        public static void LogWarning(object message, Object context = null)
        {
            if (Debug.isDebugBuild)
            {
                Debug.LogWarning($"[GAME] {message}", context);
            }
        }
        
        public static void LogError(object message, Object context = null)
        {
            if (Debug.isDebugBuild)
            {
                Debug.LogError($"[GAME] {message}", context);
            }
        }
    }
}
