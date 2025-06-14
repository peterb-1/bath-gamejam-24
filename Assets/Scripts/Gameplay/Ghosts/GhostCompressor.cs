using System;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace Gameplay.Ghosts
{
    public static class GhostCompressor 
    {
        public static string Serialize(GhostRun run) 
        {
            var json = JsonUtility.ToJson(run);
            var jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);

            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionLevel.Optimal)) 
            {
                gzip.Write(jsonBytes, 0, jsonBytes.Length);
            }
            
            return Convert.ToBase64String(output.ToArray());
        }

        public static GhostRun Deserialize(string base64Compressed)
        {
            var compressedBytes = Convert.FromBase64String(base64Compressed);

            using var input = new MemoryStream(compressedBytes);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var reader = new StreamReader(gzip);
            
            var json = reader.ReadToEnd();
            return JsonUtility.FromJson<GhostRun>(json);
        }
    }
}