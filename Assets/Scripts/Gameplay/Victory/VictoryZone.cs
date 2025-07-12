using Cysharp.Threading.Tasks;
using Gameplay.Core;
using Gameplay.Ghosts;
using Gameplay.Player;
using UnityEngine;

namespace Gameplay.Victory
{
    public class VictoryZone : MonoBehaviour
    {
        [SerializeField] 
        private Transform blackHoleTransform;

        [SerializeField] 
        private float rotationPerSecond;

        [SerializeField] 
        private AnimationCurve spinFxCurve;
        
        [SerializeField] 
        private float spinFxDuration;

        [SerializeField] 
        private float maxSpeedMultiplier;

        [SerializeField] 
        private float maxScaleMultiplier;

        private PlayerVictoryBehaviour playerVictoryBehaviour;

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerVictoryBehaviour = PlayerAccessService.Instance.PlayerVictoryBehaviour;
            playerVictoryBehaviour.OnVictorySequenceStart += HandleVictorySequenceStart;

            GhostRunner.OnSpectateVictorySequenceStart += HandleSpectateVictorySequenceStart;
        }

        private void HandleVictorySequenceStart(Vector2 _1, float _2)
        {
            RunSpinFxAsync().Forget();
        }
        
        private void HandleSpectateVictorySequenceStart()
        {
            RunSpinFxAsync().Forget();
        }

        private async UniTask RunSpinFxAsync()
        {
            var initialTime = TimeManager.Instance.UnpausedRealtimeSinceStartup;
            var startRotationSpeed = rotationPerSecond;
            var initialScale = transform.localScale;
            var timeElapsed = 0f;

            // run flash independent of timescale, since this happens during the slowdown
            while (timeElapsed < spinFxDuration)
            {
                var lerp = spinFxCurve.Evaluate(timeElapsed / spinFxDuration);

                rotationPerSecond = (1f + lerp * (maxSpeedMultiplier - 1f)) * startRotationSpeed;
                transform.localScale = (1f + lerp * (maxScaleMultiplier - 1f)) * initialScale;

                await UniTask.Yield();
                
                timeElapsed = TimeManager.Instance.UnpausedRealtimeSinceStartup - initialTime;
            }

            rotationPerSecond = startRotationSpeed;
            transform.localScale = initialScale;
        }

        private void Update()
        {
            blackHoleTransform.Rotate(blackHoleTransform.forward, Time.deltaTime * rotationPerSecond);
        }

        private void OnDestroy()
        {
            playerVictoryBehaviour.OnVictorySequenceStart -= HandleVictorySequenceStart;
            
            GhostRunner.OnSpectateVictorySequenceStart -= HandleSpectateVictorySequenceStart;
        }
    }
}
