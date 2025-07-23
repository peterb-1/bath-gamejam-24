using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Boss;
using Gameplay.Colour;
using Gameplay.Spring;
using UnityEngine;

namespace Gameplay.Core
{
    public class TimeManager : MonoBehaviour
    {
        [SerializeField] 
        private AnimationCurve slowdownCurve;

        public static TimeManager Instance { get; private set; }
        
        public float UnpausedRealtimeSinceStartup { get; private set; }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                // would usually log an error, but we expect this to happen when loading a new scene
                Destroy(gameObject);
                return;
            }

            Instance = this;
            transform.parent = null;
            DontDestroyOnLoad(this);
            
            UnpausedRealtimeSinceStartup = 0f;
            
            ColourManager.OnColourChangeStarted += HandleColourChangeStarted;
            SpringPhysicsBehaviour.OnBounce += HandleSpringBounce;
            BossHitboxBehaviour.OnBossHit += HandleBossHit;
        }
        
        private void Update()
        {
            if (PauseManager.Instance == null || !PauseManager.Instance.IsPaused)
            {
                UnpausedRealtimeSinceStartup += Time.unscaledDeltaTime;
            }
        }

        private void HandleColourChangeStarted(ColourId _, float duration)
        {
            RunSlowdownAsync(duration).Forget();
        }

        private void HandleSpringBounce(float duration)
        {
            RunSlowdownAsync(duration).Forget();
        }

        private void HandleBossHit(float duration)
        {
            RunSlowdownAsync(duration).Forget();
        }

        private async UniTask RunSlowdownAsync(float duration)
        {
            var initialTime = UnpausedRealtimeSinceStartup;
            var initialFixedDeltaTime = Time.fixedDeltaTime;
            var timeElapsed = 0f;

            while (timeElapsed < duration && !SceneLoader.Instance.IsLoading)
            {
                if (!PauseManager.Instance.IsPaused)
                {
                    var lerp = timeElapsed / duration;

                    Time.timeScale = slowdownCurve.Evaluate(lerp);
                    Time.fixedDeltaTime = initialFixedDeltaTime * Time.timeScale;
                }

                await UniTask.Yield();
                
                timeElapsed = UnpausedRealtimeSinceStartup - initialTime;
            }

            if (!PauseManager.Instance.IsPaused)
            {
                Time.timeScale = 1.0f;
            }
        }
        
        private void OnDestroy()
        {
            ColourManager.OnColourChangeStarted -= HandleColourChangeStarted;
            SpringPhysicsBehaviour.OnBounce -= HandleSpringBounce;
            BossHitboxBehaviour.OnBossHit -= HandleBossHit;
        }
    }
}
