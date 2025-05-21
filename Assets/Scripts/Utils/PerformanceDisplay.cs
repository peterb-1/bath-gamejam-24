using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;

namespace Utils
{
    public class PerformanceDisplay : MonoBehaviour
    {
        private struct FpsSample
        {
            public float Timestamp;
            public float FPS;
        }
        
        [SerializeField] 
        private TMP_Text fpsText;
        
        [SerializeField] 
        private TMP_Text memoryText;
    
        [SerializeField]
        private float sampleWindow = 5f;
    
        [SerializeField]
        private float updateInterval = 0.1f;

        private readonly Queue<FpsSample> fpsSamples = new();

        private float minFps = float.MaxValue;
        private float maxFps = float.MinValue;

        private float elapsedTime;
        private float timeSinceLastUpdate;
        private float deltaTime;

        private static PerformanceDisplay instance;

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            elapsedTime += Time.unscaledDeltaTime;
        
            var currentFps = 1.0f / deltaTime;
            var now = Time.unscaledTime;
            
            fpsSamples.Enqueue(new FpsSample { Timestamp = now, FPS = currentFps });

            while (fpsSamples.Count > 0 && now - fpsSamples.Peek().Timestamp > sampleWindow)
            {
                fpsSamples.Dequeue();
            }

            minFps = float.MaxValue;
            maxFps = float.MinValue;
        
            var fpsSum = 0f;

            foreach (var sample in fpsSamples)
            {
                minFps = Mathf.Min(minFps, sample.FPS);
                maxFps = Mathf.Max(maxFps, sample.FPS);
                
                fpsSum += sample.FPS;
            }

            var avgFps = fpsSamples.Count > 0 ? fpsSum / fpsSamples.Count : 0f;

            timeSinceLastUpdate += Time.unscaledDeltaTime;
        
            if (timeSinceLastUpdate < updateInterval) return;
        
            timeSinceLastUpdate = 0f;
        
            var totalAllocated = Profiler.GetTotalAllocatedMemoryLong();
            var monoHeap = Profiler.GetMonoHeapSizeLong();
            var monoUsed = Profiler.GetMonoUsedSizeLong();

            fpsText.text = $"FPS | Now: {currentFps:F1} | Avg: {avgFps:F1} | Min: {minFps:F1} | Max: {maxFps:F1}";
            memoryText.text = $"Memory (MB) | Allocated: {totalAllocated / (1024 * 1024):N1} | Mono Heap: {monoHeap / (1024 * 1024):N1} | Mono Used: {monoUsed / (1024 * 1024):N1}";
        }
    }
}
