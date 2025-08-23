using System;
using System.IO;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Utils
{
    public static class SaveUtils
    {
        private static readonly SemaphoreSlim FileLock = new(1, 1);
        
        public static async UniTask SaveAsync<T>(T data, string fileName) where T : class, new()
        {
            var path = Path.Combine(Application.persistentDataPath, fileName);
            var tempPath = path + ".tmp";
            var json = JsonUtility.ToJson(data, true);
            
            await FileLock.WaitAsync();

            try
            {
                await UniTask.RunOnThreadPool(() =>
                {
                    // write to temp file first to avoid corruption in event of crash
                    using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
                    using (var writer = new StreamWriter(fileStream, Encoding.UTF8))
                    {
                        writer.Write(json);
                        writer.Flush();
                        fileStream.Flush(true);
                    }

                    if (File.Exists(path))
                    {
                        File.Replace(tempPath, path, null);
                    }
                    else
                    {
                        File.Move(tempPath, path);
                    }
                });
                
                GameLogger.Log($"Saved data to path {path} successfully.");
            }
            catch (Exception e1)
            {
                GameLogger.LogError($"Failed to save data to path {path}: {e1}");
            }
            finally
            {
                FileLock.Release();
                
                // in case the temp file still exists after an error
                try
                {
                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }
                }
                catch (Exception e2)
                {
                    GameLogger.LogWarning($"Could not delete temp file {tempPath}: {e2}");
                }
            }
        }
        
        public static T Load<T>(string fileName) where T : class, new()
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
                return new T();
            }
        }
    }
}
