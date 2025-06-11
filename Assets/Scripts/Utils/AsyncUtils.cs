using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Utils
{
    public static class AsyncUtils
    {
        public static async UniTask DelayWhileFocused(TimeSpan delay)
        {
            var accumulated = 0f;

            while (accumulated < (float) delay.TotalSeconds)
            {
                while (!Application.isFocused)
                {
                    await UniTask.Yield();
                }

                var startTime = Time.realtimeSinceStartup;

                await UniTask.Yield();

                var endTime = Time.realtimeSinceStartup;

                if (Application.isFocused)
                {
                    var delta = endTime - startTime;
                    
                    accumulated += delta;
                }
            }
        }
        
        public static async UniTask AnimateWhileFocused(TimeSpan duration, AnimationCurve curve, Action<float> setter)
        {
            var accumulated = 0f;
            var total = (float) duration.TotalSeconds;

            while (accumulated < total)
            {
                while (!Application.isFocused)
                {
                    await UniTask.Yield();
                }

                var startTime = Time.realtimeSinceStartup;

                await UniTask.Yield();

                var endTime = Time.realtimeSinceStartup;

                if (Application.isFocused)
                {
                    var delta = endTime - startTime;
                    
                    accumulated += delta;
                    
                    setter.Invoke(curve.Evaluate(Mathf.Clamp01(accumulated / total)));
                }
            }
            
            setter.Invoke(curve.Evaluate(1f));
        }
    }
}