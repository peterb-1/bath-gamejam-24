using System.IO;
using System.IO.Compression;
using UnityEngine;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace Gameplay.Ghosts
{
    public static class GhostCompressor 
    {
        public static byte[] Serialize(GhostRun run) 
        {
            var json = JsonUtility.ToJson(run);
            var jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);

            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionLevel.Optimal)) 
            {
                gzip.Write(jsonBytes, 0, jsonBytes.Length);
            }
            
            return output.ToArray();
        }

        public static GhostRun Deserialize(byte[] compressed)
        {
            using var input = new MemoryStream(compressed);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var reader = new StreamReader(gzip);
            
            var json = reader.ReadToEnd();
            
            return JsonUtility.FromJson<GhostRun>(json);
        }
    }
}