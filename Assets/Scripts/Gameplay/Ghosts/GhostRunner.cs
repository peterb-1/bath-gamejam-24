using System;
using System.Collections.Generic;
using Audio;
using Core;
using Core.Saving;
using Cysharp.Threading.Tasks;
using Gameplay.Camera;
using Gameplay.Colour;
using Gameplay.Core;
using Gameplay.Dash;
using Gameplay.Drone;
using Gameplay.Player;
using UnityEngine;
using Utils;

namespace Gameplay.Ghosts
{
    public class GhostRunner : MonoBehaviour
    {
        public const string GHOST_DATA_KEY = "GhostData";
        public const string LOAD_FROM_LEADERBOARD_KEY = "LoadFromLeaderboard";
        public const string SPECTATE_KEY = "Spectate";
        
        [SerializeField]
        private SpriteRenderer spriteRenderer;
        
        [SerializeField]
        private Animator animator;

        [SerializeField] 
        private AnimationCurve shrinkCurve;
        
        [SerializeField] 
        private AnimationCurve flashCurve;
        
        [SerializeField] 
        private ColourDatabase colourDatabase;

        [SerializeField] 
        private float flashDuration;

        private List<GhostFrame> frames;
        private List<GhostEvent> ghostEvents;
        private ColourId currentColour;

        private float playbackTime;
        private float victoryTime;
        private int displayMilliseconds;
        private int currentAnimatorStateHash;
        private int currentIndex;
        private int currentEventIndex;
        private bool isFinishing;
        private Vector2 lastPosition;
        
        public static bool IsSpectating { get; private set; }
        public static Vector2 Velocity { get; private set; }
        
        public static event Action<ColourId> OnGhostColourChangedWhileSpectating;
        public static event Action OnSpectateVictorySequenceStart;
        public static event Action<int> OnSpectateVictorySequenceFinish;
        public static event Action OnGhostFoundCollectible;

        private async void Awake()
        {
            // ghost is spawned under the camera so the camera doesn't zip across the screen, but want it to be separate for everything else
            transform.parent = null;
            
            GhostRun ghostRun = null;

            var hasLoadContext = SceneLoader.Instance.SceneLoadContext != null;

            if (hasLoadContext && SceneLoader.Instance.SceneLoadContext.TryGetCustomData(GHOST_DATA_KEY, out GhostContext ghostContext))
            {
                ghostRun = ghostContext.GhostRun;
                displayMilliseconds = ghostContext.DisplayMilliseconds;
            }

            var shouldLoadLocalGhost = false;

            if (hasLoadContext)
            {
                SceneLoader.Instance.SceneLoadContext.TryGetCustomData(LOAD_FROM_LEADERBOARD_KEY, out shouldLoadLocalGhost);
            }

            if (SaveManager.Instance.SaveData.PreferenceData.TryGetValue(SettingId.LocalGhost, out bool areLocalGhostsEnabled))
            {
                shouldLoadLocalGhost |= areLocalGhostsEnabled;
            }

            if (shouldLoadLocalGhost && ghostRun == null && SceneLoader.Instance.CurrentLevelData is { GhostData: not null })
            {
                ghostRun = GhostCompressor.Deserialize(SceneLoader.Instance.CurrentLevelData.GhostData);
            }

            if (ghostRun == null)
            {
                Destroy(gameObject);
            }
            else
            {
                frames = ghostRun.frames;
                ghostEvents = ghostRun.droneKills;
                victoryTime = ghostRun.victoryTime;
                
                if (frames is { Count: > 0 } && colourDatabase.TryGetColourConfig(frames[0].colourId, out var colourConfig))
                {
                    spriteRenderer.color = colourConfig.PlayerColour;
                }
            }

            if (hasLoadContext && SceneLoader.Instance.SceneLoadContext.TryGetCustomData(SPECTATE_KEY, out bool isSpectating))
            {
                IsSpectating = isSpectating;

                if (!isSpectating) return;
                
                await UniTask.WaitUntil(() => PlayerAccessService.IsReady() && CameraAccessService.IsReady());

                CameraAccessService.Instance.CameraFollow.OverrideTarget(transform);
                PlayerAccessService.Instance.DisablePlayerBehavioursForSpectate();
            }
            else
            {
                IsSpectating = false;
            }
        }

        private void Update() 
        {
            if (frames == null || frames.Count == 0 || PauseManager.Instance.IsPaused) return;
            
            var currentPosition = transform.position.xy();
            
            Velocity = currentPosition == lastPosition 
                ? Vector2.zero 
                : (currentPosition - lastPosition) / Time.deltaTime;

            lastPosition = currentPosition;

            // increment to current frame
            playbackTime += Time.deltaTime;
            playbackTime = Mathf.Min(playbackTime, frames[^1].time);

            while (currentIndex < frames.Count - 1 && frames[currentIndex + 1].time <= playbackTime) 
            {
                currentIndex++;
            }

            // want to still see the player move (especially on quick restart) but don't want audio events if loading
            if (!SceneLoader.Instance.IsLoading)
            {
                RunSpectatorEvents();
            }

            // if we've reached the end of the ghost
            if (currentIndex >= frames.Count - 1) 
            {
                return;
            }

            // otherwise, interpolate between closest frames
            var frameA = frames[currentIndex];
            var frameB = frames[currentIndex + 1];

            var lerp = Mathf.InverseLerp(frameA.time, frameB.time, playbackTime);
            var trans = transform;
            var transformAngles = trans.rotation.eulerAngles;
            
            trans.position = Vector2.Lerp(frameA.position, frameB.position, lerp);
            transformAngles.z = AngleUtils.AngleLerp(frameA.zRotation, frameB.zRotation, lerp);
            trans.rotation = Quaternion.Euler(transformAngles);

            var animationStateHash = lerp < 0.5f ? frameA.animationStateHash : frameB.animationStateHash;
            if (animationStateHash != currentAnimatorStateHash)
            {
                animator.Play(animationStateHash);
                currentAnimatorStateHash = animationStateHash;
            }
            
            var colour = lerp < 0.5f ? frameA.colourId : frameB.colourId;
            if (colour != currentColour && colourDatabase.TryGetColourConfig(colour, out var colourConfig))
            {
                currentColour = colour;
                spriteRenderer.color = colourConfig.PlayerColour;
                
                RunFlashAsync().Forget();

                // time-slow events are VERY dangerous if loading
                if (IsSpectating && !SceneLoader.Instance.IsLoading)
                {
                    OnGhostColourChangedWhileSpectating?.Invoke(currentColour);
                }
            }

            spriteRenderer.flipX = lerp < 0.5f ? !frameA.isFacingRight : !frameB.isFacingRight;
            
            // check if we need to shrink the player as they approach the end
            if (playbackTime > victoryTime && !isFinishing)
            {
                isFinishing = true;
                
                RunVictoryShrinkAsync().Forget();
            }
        }

        private void RunSpectatorEvents()
        {
            if (!IsSpectating) return;
            
            while (currentEventIndex < ghostEvents.Count && ghostEvents[currentEventIndex].time <= playbackTime)
            {
                var currentEvent = ghostEvents[currentEventIndex];
                
                switch (currentEvent.type)
                {
                    case GhostEventType.Jump:
                        AudioManager.Instance.Play(AudioClipIdentifier.Jump);
                        break;
                    case GhostEventType.Land:
                        AudioManager.Instance.Play(AudioClipIdentifier.Land);
                        break;
                    case GhostEventType.Dash:
                        DashTrackerService.Instance.NotifyGhostPerformedDash();
                        break;
                    case GhostEventType.DashCollection:
                        var orbId = currentEvent.data;
                        DashTrackerService.Instance.TryCollectFromSpectatorGhost(orbId);
                        break;
                    case GhostEventType.DroneKill:
                        var droneId = currentEvent.data;
                        DroneTrackerService.KillDroneFromSpectatorGhost(droneId, transform.position.xy());
                        break;
                    case GhostEventType.ZiplineHook:
                        AudioManager.Instance.Play(AudioClipIdentifier.ZiplineAttach);
                        break;
                    case GhostEventType.ZiplineUnhook:
                        AudioManager.Instance.Stop(AudioClipIdentifier.ZiplineAttach);
                        AudioManager.Instance.Play(AudioClipIdentifier.ZiplineDetach);
                        break;
                    case GhostEventType.CollectibleFound:
                        OnGhostFoundCollectible?.Invoke();
                        break;
                }
                
                currentEventIndex++;
            }
        }

        private async UniTask RunVictoryShrinkAsync()
        {
            if (IsSpectating)
            {
                AudioManager.Instance.Play(AudioClipIdentifier.Victory);
                
                OnSpectateVictorySequenceStart?.Invoke();
            }

            var timeElapsed = 0f;
            var trans = transform;
            var duration = frames[^1].time - playbackTime;
            
            while (timeElapsed < duration)
            {
                var lerp = shrinkCurve.Evaluate(timeElapsed / duration);
                
                trans.localScale = (1.0f - lerp) * Vector3.one;

                await UniTask.Yield();

                timeElapsed += Time.deltaTime;
            }
            
            trans.localScale = Vector3.zero;

            if (IsSpectating)
            {
                // fallback to the accumulated time if display time couldn't be found for some reason
                var milliseconds = displayMilliseconds == 0f ? playbackTime.ToMilliseconds() : displayMilliseconds;
                
                OnSpectateVictorySequenceFinish?.Invoke(milliseconds);
            }
        }
        
        private async UniTask RunFlashAsync()
        {
            var initialTime = Time.time;
            var startColour = spriteRenderer.color;
            var timeElapsed = 0f;

            while (timeElapsed < flashDuration)
            {
                var lerp = flashCurve.Evaluate(timeElapsed / flashDuration);
                var colour = (1f - lerp) * startColour + lerp * Color.white;

                spriteRenderer.color = colour;

                await UniTask.Yield();
                
                timeElapsed = Time.time - initialTime;
            }

            spriteRenderer.color = startColour;
        }
    }
}