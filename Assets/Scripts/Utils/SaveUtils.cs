using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Utils
{
    public static class SaveUtils
    {
        public static void Save<T>(T data, string fileName) where T : class
        {
            var path = Path.Combine(Application.persistentDataPath, fileName);
            var json = JsonUtility.ToJson(data);

            try
            {
                using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
                using var writer = new StreamWriter(fileStream, Encoding.UTF8);
                
                writer.Write(json);
                
                GameLogger.Log($"Saved data to path {path} successfully.");
            }
            catch (Exception e)
            {
                GameLogger.LogError($"Failed to save data to path {path}: {e}");
            }
        }
        
        public static T Load<T>(string fileName) where T : class
        {
            var path = Path.Combine(Application.persistentDataPath, fileName);
            
            try
            {
                using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                using var reader = new StreamReader(fileStream, Encoding.UTF8);

                var json = reader.ReadToEnd();
                var data = JsonUtility.FromJson<T>(json);
                
                GameLogger.Log($"Read data from path {path} successfully.");
                return data;
            }
            catch (Exception e)
            {
                GameLogger.LogError($"Failed to read data from path {path}: {e}");
                return null;
            }
        }
    }
}
