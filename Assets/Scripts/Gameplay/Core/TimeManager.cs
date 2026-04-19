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
        
        [SerializeField]
        private float freezeFrameDuration;

        public static TimeManager Instance { get; private set; }
        
        public float UnpausedRealtimeSinceStartup { get; private set; }
        
        public bool IsFrozen { get; private set; }
        
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
                if (!PauseManager.Instance.IsPaused && !IsFrozen)
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

        public async UniTask FreezeFrameAsync()
        {
            IsFrozen = true;

            Time.timeScale = 0f;

            var initialTime = UnpausedRealtimeSinceStartup;
            await UniTask.WaitUntil(() => UnpausedRealtimeSinceStartup - initialTime >= freezeFrameDuration);

            IsFrozen = false;
            
            if (!PauseManager.Instance.IsPaused)
            {
                Time.timeScale = 1f;
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
