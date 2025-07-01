using System;
using System.Threading;
using Core;
using Core.Saving;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Hardware
{
    public class RumbleManager : MonoBehaviour
    {
        private bool isPaused;
        
        private float currentLowFreq;
        private float currentHighFreq;

        private CancellationTokenSource oneShotCts;
        private CancellationTokenSource continuousCts;

        public static RumbleManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            transform.parent = null;
            DontDestroyOnLoad(this);

            SceneLoader.OnSceneLoadStart += HandleSceneLoadStart;
        }

        public void Pause()
        {
            if (!isPaused && Gamepad.current != null)
            {
                isPaused = true;
                
                Gamepad.current.SetMotorSpeeds(0f, 0f);
            }
        }

        public void Unpause()
        {
            if (isPaused && Gamepad.current != null)
            {
                isPaused = false;
                
                SetMotor(currentLowFreq, currentHighFreq);
            }
        }

        public void Rumble(RumbleConfig rumbleConfig)
        {
            CancelRumble();
            OneShotRumbleAsync(rumbleConfig, oneShotCts.Token).Forget();
        }

        public void Rumble(ContinuousRumbleConfig rumbleConfig)
        {
            CancelRumble();
            ContinuousRumbleAsync(rumbleConfig, continuousCts.Token).Forget();
        }

        public void StopRumble()
        {
            CancelRumble();
            SetMotor(0f, 0f);
        }
        
        private void HandleSceneLoadStart()
        {
            isPaused = false;
            
            StopRumble();
        }
        
        private void SetMotor(float low, float high)
        {
            currentLowFreq = low;
            currentHighFreq = high;

            if (!isPaused && Gamepad.current != null)
            {
                Gamepad.current.SetMotorSpeeds(low, high);
            }
        }
        
        private void CancelRumble()
        {
            continuousCts?.Cancel();
            continuousCts?.Dispose();
            continuousCts = new CancellationTokenSource();
            
            oneShotCts?.Cancel();
            oneShotCts?.Dispose();
            oneShotCts = new CancellationTokenSource();
        }

        private async UniTask OneShotRumbleAsync(RumbleConfig config, CancellationToken token)
        {
            if (Gamepad.current == null) return;
            
            if (!SaveManager.Instance.SaveData.PreferenceData.TryGetValue(SettingId.RumbleStrength, out float rumbleMultiplier))
            {
                rumbleMultiplier = 1f;
            }

            var timeElapsed = 0f;

            while (timeElapsed < config.Duration)
            {
                if (token.IsCancellationRequested || Gamepad.current == null)
                    break;

                var lerp = timeElapsed / config.Duration;
                var lowFrequency = config.LowFrequencyCurve.Evaluate(lerp);
                var highFrequency = config.HighFrequencyCurve.Evaluate(lerp);

                SetMotor(lowFrequency * rumbleMultiplier, highFrequency * rumbleMultiplier);

                await UniTask.Yield(PlayerLoopTiming.Update, token);
                timeElapsed += Time.deltaTime;
            }

            if (!token.IsCancellationRequested)
            {
                SetMotor(0f, 0f);
            }
        }

        private async UniTask ContinuousRumbleAsync(ContinuousRumbleConfig rumbleConfig, CancellationToken token)
        {
            if (Gamepad.current == null) return;

            if (!SaveManager.Instance.SaveData.PreferenceData.TryGetValue(SettingId.RumbleStrength, out float rumbleMultiplier))
            {
                rumbleMultiplier = 1f;
            }

            SetMotor(rumbleConfig.LowFrequency * rumbleMultiplier, rumbleConfig.HighFrequency * rumbleMultiplier);

            try
            {
                while (true)
                {
                    if (token.IsCancellationRequested) break;

                    if (isPaused && Gamepad.current != null)
                    {
                        SetMotor(0f, 0f);
                        
                        await UniTask.WaitUntil(() => !isPaused || token.IsCancellationRequested, cancellationToken: token);
                        if (token.IsCancellationRequested) break;
                        
                        SetMotor(rumbleConfig.LowFrequency * rumbleMultiplier, rumbleConfig.HighFrequency * rumbleMultiplier);
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            catch (OperationCanceledException) {}

            SetMotor(0f, 0f);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SetMotor(0f, 0f);

                oneShotCts?.Cancel();
                oneShotCts?.Dispose();

                continuousCts?.Cancel();
                continuousCts?.Dispose();

                Instance = null;
            }
        }
    }
}
