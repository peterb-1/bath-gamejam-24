using System;
using System.Collections.Generic;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Colour;
using Gameplay.Core;
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
            
            audioDatabase.Initialise();
            audioSourcePool = new Pool<AudioSource>(audioSourcePrefab, defaultCapacity: 20);

            ColourManager.OnColourChangeStarted += HandleColourChangeStarted;
            SceneLoader.OnSceneLoadStart += HandleSceneLoadStart;
            SceneLoader.OnSceneLoaded += HandleSceneLoaded;
            
            HandleSceneLoaded();
        }

        private void HandleSceneLoadStart()
        {
            // stop any "long" sounds
            Stop(AudioClipIdentifier.ZiplineAttach);
            
            DisableFxAsync().Forget();
        }

        private void HandleColourChangeStarted(ColourId _, float duration)
        {
            RunFxCurveAsync(duration).Forget();
        }
        
        private void HandleSceneLoaded()
        {
            GetPlayerBehavioursAsync().Forget();
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

        private void HandleDeathSequenceStart()
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
            var initialTime = TimeManager.Instance.UnpausedRealtimeSinceStartup;
            var timeElapsed = 0f;

            // run fx curve independent of timescale, since this happens during the slowdown
            while (timeElapsed < duration && playerDeathBehaviour.IsAlive)
            {
                var lerp = fxCurve.Evaluate(timeElapsed / duration);

                audioMixer.SetFloat(LOW_PASS_CUTOFF, (1f - lerp) * UNFILTERED_FREQUENCY);
                audioMixer.SetFloat(FLANGER_DRY, 1f - lerp);
                audioMixer.SetFloat(FLANGER_WET, lerp);
                
                await UniTask.Yield();
                
                timeElapsed = TimeManager.Instance.UnpausedRealtimeSinceStartup - initialTime;
            }

            if (playerDeathBehaviour.IsAlive)
            {
                audioMixer.SetFloat(LOW_PASS_CUTOFF, UNFILTERED_FREQUENCY);
                audioMixer.SetFloat(FLANGER_DRY, 1f);
                audioMixer.SetFloat(FLANGER_WET, 0f);
            }
        }

        private async UniTask RunFilterCurveAsync(AnimationCurve curve, float duration)
        {
            var initialTime = TimeManager.Instance.UnpausedRealtimeSinceStartup;
            var timeElapsed = 0f;
            
            audioMixer.GetFloat(LOW_PASS_CUTOFF, out var startFrequency);

            // may die during slowdown, so use real time
            while (timeElapsed < duration)
            {
                var lerp = curve.Evaluate(timeElapsed / duration);

                audioMixer.SetFloat(LOW_PASS_CUTOFF, lerp * startFrequency);
                
                await UniTask.Yield();
                
                timeElapsed = TimeManager.Instance.UnpausedRealtimeSinceStartup - initialTime;
            }
        }

        private async UniTask DisableFxAsync()
        {
            var initialTime = TimeManager.Instance.UnpausedRealtimeSinceStartup;
            var timeElapsed = 0f;
            
            audioMixer.GetFloat(LOW_PASS_CUTOFF, out var startFrequency);
            
            while (timeElapsed < disableFxDuration)
            {
                var lerp = disableFxCurve.Evaluate(timeElapsed / disableFxDuration);

                audioMixer.SetFloat(LOW_PASS_CUTOFF, (1f - lerp) * startFrequency + lerp * UNFILTERED_FREQUENCY);
                
                await UniTask.Yield();
                
                timeElapsed = TimeManager.Instance.UnpausedRealtimeSinceStartup - initialTime;
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
        }
    }
}
