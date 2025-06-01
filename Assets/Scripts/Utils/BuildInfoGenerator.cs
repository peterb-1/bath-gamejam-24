using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utils
{
    public static class BuildInfoGenerator
    {
        public const string BUILD_INFO_PATH = "BuildInfo";
        
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void GenerateBuildInfo()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = "rev-parse --short HEAD",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WorkingDirectory = Directory.GetCurrentDirectory()
                    }
                };

                process.Start();
                var hash = process.StandardOutput.ReadLine()?.Trim();
                process.WaitForExit();

                if (string.IsNullOrEmpty(hash)) return;
                
                File.WriteAllText($"Assets/Resources/{BUILD_INFO_PATH}.txt", hash);
                AssetDatabase.Refresh();
            }
            catch
            {
                Debug.LogWarning("Failed to generate build info from Git hash!");
            }
        }
#endif
    }
}