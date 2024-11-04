using Cysharp.Threading.Tasks;
using Gameplay.Colour;
using UnityEngine;

namespace Gameplay.Core
{
    public class TimeManager : MonoBehaviour
    {
        [SerializeField] 
        private AnimationCurve slowdownCurve;
        
        private void Awake()
        {
            ColourManager.OnColourChangeStarted += HandleColourChangeStarted;
        }

        private void HandleColourChangeStarted(ColourId _, float duration)
        {
            RunSlowdownAsync(duration).Forget();
        }

        private async UniTask RunSlowdownAsync(float duration)
        {
            var initialTime = Time.realtimeSinceStartup;
            var initialFixedDeltaTime = Time.fixedDeltaTime;
            var timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                var lerp = timeElapsed / duration;

                Time.timeScale = slowdownCurve.Evaluate(lerp);
                Time.fixedDeltaTime = initialFixedDeltaTime * Time.timeScale;

                await UniTask.Yield();
                
                timeElapsed = Time.realtimeSinceStartup - initialTime;
            }
        }
        
        private void OnDestroy()
        {
            ColourManager.OnColourChangeStarted -= HandleColourChangeStarted;
        }
    }
}
