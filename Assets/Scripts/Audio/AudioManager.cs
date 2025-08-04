using System;
using System.Collections.Generic;
using System.Threading;
using Core;
using Core.Saving;
using Cysharp.Threading.Tasks;
using Gameplay.Colour;
using Gameplay.Core;
using Gameplay.Ghosts;
using Gameplay.Player;
using UnityEngine;
using UnityEngine.Audio;
using Utils;

namespace Audio
{
    public class AudioManager : MonoBehaviour
    {
        private const float UNFILTERED_FREQUENCY = 22000;
        private const float PLAYBACK_END_TOLERANCE = 0.1f;
        private const float PLAYER_ACCESS_TIMEOUT = 1f;
        
        private const string LOW_PASS_CUTOFF = "LowPassCutoff";
        private const string FLANGER_DRY = "FlangerDry";
        private const string FLANGER_WET = "FlangerWet";
        
        [SerializeField] 
        private AudioDatabase audioDatabase;

        [SerializeField] 
        private AudioSource audioSourcePrefab;

        [SerializeField] 
        private AudioMixer audioMixer;

        [SerializeField] 
        private AnimationCurve fxCurve;

        [SerializeField] 
        private AnimationCurve endLevelCurve;

        [SerializeField] 
        private AnimationCurve deathCurve;
        
        [SerializeField] 
        private AnimationCurve disableFxCurve;

        [SerializeField] 
        private float endLevelDuration;
        
        [SerializeField] 
        private float deathDuration;
        
        [SerializeField] 
        private float disableFxDuration;

        public static AudioManager Instance { get; private set; }

        // store audio source as well to uniquely identify multiple instances of the same clip
        private readonly HashSet<(AudioClipData, AudioSource)> playingSfxSources = new();
        private readonly HashSet<(MusicData, AudioSource)> playingMusicSources = new();
        private Pool<AudioSource> audioSourcePool;
        private PlayerDeathBehaviour playerDeathBehaviour;
        private PlayerVictoryBehaviour playerVictoryBehaviour;

        private CancellationTokenSource fxCurveCts;

        private async void Awake()
        {
            if (Instance != null && Instance != this)
            {
                // would usually log an error, but we expect this to happen when loading a new scene
                Destroy(gameObject);
                return;
            }

            audioDatabase.Initialise();
            audioSourcePool = new Pool<AudioSource>(audioSourcePrefab, defaultCapacity: 20);

            ColourManager.OnColourChangeStarted += HandleColourChangeStarted;
            SceneLoader.OnSceneLoadStart += HandleSceneLoadStart;
            SceneLoader.OnSceneLoaded += HandleSceneLoaded;
            GhostRunner.OnSpectateVictorySequenceStart += HandleSpectateVictorySequenceStart;
            
            HandleSceneLoaded();
            
            await InitialiseSettingsAsync();

            Instance = this;
            transform.parent = null;
            DontDestroyOnLoad(this);
        }

        private void HandleSceneLoadStart()
        {
            // stop any "long" sounds
            Stop(AudioClipIdentifier.ZiplineAttach);
        }

        private void HandleColourChangeStarted(ColourId _, float duration)
        {
            RunFxCurveAsync(duration).Forget();
        }
        
        private void HandleSceneLoaded()
        {
            GetPlayerBehavioursAsync().Forget();
            DisableFxAsync().Forget();
        }
        
        private async UniTask InitialiseSettingsAsync()
        {
            await UniTask.WaitUntil(() => SaveManager.IsReady);

            var preferenceData = SaveManager.Instance.SaveData.PreferenceData;
            
            preferenceData.OnSettingChanged += HandleSettingChanged;
            
            InitialiseSetting(SettingId.MasterVolume);
            InitialiseSetting(SettingId.MusicVolume);
            InitialiseSetting(SettingId.SfxVolume);
        }

        private void InitialiseSetting(SettingId settingId)
        {
            if (SaveManager.Instance.SaveData.PreferenceData.TryGetValue(settingId, out object value))
            {
                HandleSettingChanged(settingId, value);
            }
        }

        private void HandleSettingChanged(SettingId settingId, object value)
        {
            if (value is not float sliderVolume) return;
            
            var floatNames = settingId switch
            {
                SettingId.MasterVolume => new[] {"MasterVolume"},
                SettingId.MusicVolume => new[] {"MusicVolume"},
                SettingId.SfxVolume => new[] {"SfxVolume", "GameplayUnfilteredVolume", "UIVolume"},
                _ => null
            };

            if (floatNames == null) return;

            var t = Mathf.Pow(sliderVolume, 0.18f);
            var volume = Mathf.Lerp(-80f, 3f, t);

            foreach (var floatName in floatNames)
            {
                audioMixer.SetFloat(floatName, volume);
            }
        }

        private async UniTask GetPlayerBehavioursAsync()
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(PLAYER_ACCESS_TIMEOUT));
            
            var isCancelled = await UniTask.WaitUntil(PlayerAccessService.IsReady, cancellationToken: cts.Token).SuppressCancellationThrow();

            if (!isCancelled)
            {
                playerDeathBehaviour = PlayerAccessService.Instance.PlayerDeathBehaviour;
                playerVictoryBehaviour = PlayerAccessService.Instance.PlayerVictoryBehaviour;

                playerDeathBehaviour.OnDeathSequenceStart += HandleDeathSequenceStart;
                playerVictoryBehaviour.OnVictorySequenceStart += HandleVictorySequenceStart;
            }
        }

        private void HandleDeathSequenceStart(PlayerDeathSource _)
        {
            playerDeathBehaviour.OnDeathSequenceStart -= HandleDeathSequenceStart;
            playerVictoryBehaviour.OnVictorySequenceStart -= HandleVictorySequenceStart;
            
            RunFilterCurveAsync(deathCurve, deathDuration).Forget();
        }
        
        private void HandleVictorySequenceStart(Vector2 _1, float _2)
        {
            playerDeathBehaviour.OnDeathSequenceStart -= HandleDeathSequenceStart;
            playerVictoryBehaviour.OnVictorySequenceStart -= HandleVictorySequenceStart;
            
            RunFilterCurveAsync(endLevelCurve, endLevelDuration).Forget();
        }
        
        private void HandleSpectateVictorySequenceStart()
        {
            playerDeathBehaviour.OnDeathSequenceStart -= HandleDeathSequenceStart;
            playerVictoryBehaviour.OnVictorySequenceStart -= HandleVictorySequenceStart;
            
            RunFilterCurveAsync(endLevelCurve, endLevelDuration).Forget();
        }

        public void Play(AudioClipIdentifier identifier, Action onFinishedCallback = null)
        {
            if (audioDatabase.TryGetClipData(identifier, out var clipData))
            {
                PlayClipAsync(clipData, onFinishedCallback).Forget();
            }
            else
            {
                GameLogger.LogError($"Cannot find audio clip with identifier {identifier}!", this);
            }
        }

        public void PlayMusic(MusicIdentifier identifier)
        {
            if (audioDatabase.TryGetMusicData(identifier, out var musicData))
            {
                PlayMusic(musicData);
            }
            else
            {
                GameLogger.LogError($"Cannot find music data for identifier {identifier}!", this);
            }
        }

        private async UniTask PlayAsync(AudioClipIdentifier identifier)
        {
            if (audioDatabase.TryGetClipData(identifier, out var clipData))
            {
                await PlayClipAsync(clipData);
            }
            else
            {
                GameLogger.LogError($"Cannot find audio clip with identifier {identifier}!", this);
            }
        }

        public void Stop(AudioClipIdentifier identifier)
        {
            var itemsToRemove = new List<(AudioClipData, AudioSource)>();

            foreach (var (clipData, audioSource) in playingSfxSources)
            {
                if (clipData.Identifier == identifier)
                {
                    audioSource.Stop();
                    audioSourcePool.Release(audioSource);

                    itemsToRemove.Add((clipData, audioSource));
                }
            }

            foreach (var item in itemsToRemove)
            {
                playingSfxSources.Remove(item);
            }
        }
        
        public void Pause()
        {
            foreach (var (_, source) in playingSfxSources)
            {
                source.Pause();
            }
        }

        public void Unpause()
        {
            foreach (var (_, source) in playingSfxSources)
            {
                if (!source.isPlaying)
                {
                    source.UnPause();
                }
            }
        }

        private async UniTask PlayClipAsync(AudioClipData clipData, Action onFinishedCallback = null)
        {
            var audioSource = audioSourcePool.Get();
            var duration = clipData.AudioClip.length;
            
            audioSource.clip = clipData.AudioClip;
            audioSource.loop = clipData.IsLooping;
            audioSource.volume = clipData.Volume;
            audioSource.outputAudioMixerGroup = clipData.MixerGroup;
            audioSource.Play();
            
            playingSfxSources.Add((clipData, audioSource));

            await UniTask.WaitUntil(() => audioSource.time >= duration - PLAYBACK_END_TOLERANCE);

            // check if it's still playing and hasn't been stopped externally
            if (playingSfxSources.Contains((clipData, audioSource)))
            {
                playingSfxSources.Remove((clipData, audioSource));
            
                audioSourcePool.Release(audioSource);
            
                onFinishedCallback?.Invoke();
            }
        }
        
        private void PlayMusic(MusicData musicData)
        {
            var itemsToRemove = new List<(MusicData, AudioSource)>();
            
            foreach (var (data, audioSource) in playingMusicSources)
            {
                if (data.Identifier != musicData.Identifier)
                {
                    audioSource.Stop();
                    audioSourcePool.Release(audioSource);
                    
                    itemsToRemove.Add((data, audioSource));
                }
            }

            // if we have nothing to remove, then we're not changing districts
            if (playingMusicSources.Count > 0 && itemsToRemove.Count == 0) return;
            
            // TODO: SFX for switching music? should probably be punctuated by button click, but might want to lowpass or similar
            
            foreach (var item in itemsToRemove)
            {
                playingMusicSources.Remove(item);
            }

            foreach (var clip in musicData.ParallelAudioClips)
            {
                var audioSource = audioSourcePool.Get();
                
                audioSource.clip = clip;
                audioSource.volume = musicData.Volume;
                audioSource.outputAudioMixerGroup = musicData.MixerGroup;
                audioSource.loop = true;
                
                audioSource.Play();
                
                playingMusicSources.Add((musicData, audioSource));
            }
        }
        
        private async UniTask RunFxCurveAsync(float duration)
        {
            fxCurveCts?.Cancel();
            fxCurveCts?.Dispose();
            fxCurveCts = new CancellationTokenSource();

            var token = fxCurveCts.Token;
            var initialTime = TimeManager.Instance.UnpausedRealtimeSinceStartup;
            var timeElapsed = 0f;

            // run fx curve independent of timescale, since this happens during the slowdown
            while (timeElapsed < duration && playerDeathBehaviour.IsAlive && !token.IsCancellationRequested)
            {
                var lerp = fxCurve.Evaluate(timeElapsed / duration);

                audioMixer.SetFloat(LOW_PASS_CUTOFF, (1f - lerp) * UNFILTERED_FREQUENCY);
                audioMixer.SetFloat(FLANGER_DRY, 1f - lerp);
                audioMixer.SetFloat(FLANGER_WET, lerp);
                
                await UniTask.Yield();
                
                timeElapsed = TimeManager.Instance.UnpausedRealtimeSinceStartup - initialTime;
            }

            if (playerDeathBehaviour.IsAlive && !token.IsCancellationRequested)
            {
                audioMixer.SetFloat(LOW_PASS_CUTOFF, UNFILTERED_FREQUENCY);
                audioMixer.SetFloat(FLANGER_DRY, 1f);
                audioMixer.SetFloat(FLANGER_WET, 0f);
            }
        }

        private async UniTask RunFilterCurveAsync(AnimationCurve curve, float duration)
        {
            fxCurveCts?.Cancel();
            fxCurveCts?.Dispose();
            fxCurveCts = new CancellationTokenSource();

            var token = fxCurveCts.Token;
            var initialTime = TimeManager.Instance.UnpausedRealtimeSinceStartup;
            var targetProportion = curve.Evaluate(1f);
            var targetFrequency = targetProportion * UNFILTERED_FREQUENCY;
            var timeElapsed = 0f;
            
            audioMixer.GetFloat(LOW_PASS_CUTOFF, out var startFrequency);

            // may die during slowdown, so use real time
            while (timeElapsed < duration && !token.IsCancellationRequested)
            {
                var lerp = (curve.Evaluate(timeElapsed / duration) - targetProportion) / (1f - targetProportion);

                audioMixer.SetFloat(LOW_PASS_CUTOFF, Mathf.Lerp(targetFrequency, startFrequency, lerp));
                
                await UniTask.Yield();
                
                timeElapsed = TimeManager.Instance.UnpausedRealtimeSinceStartup - initialTime;
            }
        }

        private async UniTask DisableFxAsync()
        {
            // because seemingly second frame after load has a very high unscaled delta time?
            await UniTask.DelayFrame(2);
            
            fxCurveCts?.Cancel();
            fxCurveCts?.Dispose();
            fxCurveCts = new CancellationTokenSource();
            
            var token = fxCurveCts.Token;
            var initialTime = TimeManager.Instance.UnpausedRealtimeSinceStartup;
            var timeElapsed = 0f;
            
            audioMixer.GetFloat(LOW_PASS_CUTOFF, out var startFrequency);
            audioMixer.GetFloat(FLANGER_DRY, out var startFlangerDry);
            audioMixer.GetFloat(FLANGER_WET, out var startFlangerWet);
            
            while (timeElapsed < disableFxDuration && !token.IsCancellationRequested)
            {
                var lerp = disableFxCurve.Evaluate(timeElapsed / disableFxDuration);

                audioMixer.SetFloat(LOW_PASS_CUTOFF, (1f - lerp) * startFrequency + lerp * UNFILTERED_FREQUENCY);
                audioMixer.SetFloat(FLANGER_DRY, (1f - lerp) * startFlangerDry + lerp);
                audioMixer.SetFloat(FLANGER_WET, (1f - lerp) * startFlangerWet);
                
                GameLogger.Log($"setting mixer values on disable curve {(1f - lerp) * startFrequency + lerp * UNFILTERED_FREQUENCY} {(1f - lerp) * startFlangerDry + lerp} {(1f - lerp) * startFlangerWet}");
                
                await UniTask.Yield();
                
                timeElapsed = TimeManager.Instance.UnpausedRealtimeSinceStartup - initialTime;
                
                GameLogger.Log($"time elapsed is {timeElapsed}, delta time {Time.deltaTime} (unscaled {Time.unscaledDeltaTime}) timescale {Time.timeScale}");
            }
            
            audioMixer.SetFloat(LOW_PASS_CUTOFF, UNFILTERED_FREQUENCY);
            audioMixer.SetFloat(FLANGER_DRY, 1f);
            audioMixer.SetFloat(FLANGER_WET, 0f);
        }
        
        public static bool IsReady() => Instance != null;

        private void OnDestroy()
        {
            if (Instance != this) return;
            Instance = null;
            
            ColourManager.OnColourChangeStarted -= HandleColourChangeStarted;
            SceneLoader.OnSceneLoadStart -= HandleSceneLoadStart;
            SceneLoader.OnSceneLoaded -= HandleSceneLoaded;
            GhostRunner.OnSpectateVictorySequenceStart -= HandleSpectateVictorySequenceStart;

            SaveManager.Instance.SaveData.PreferenceData.OnSettingChanged -= HandleSettingChanged;
        }
    }
}
