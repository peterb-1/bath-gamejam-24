using System.Collections.Generic;
using Core;
using Gameplay.Colour;
using Gameplay.Drone;
using Gameplay.Player;
using UnityEngine;
using Utils;

namespace Gameplay.Ghosts
{
    public class GhostWriter : MonoBehaviour
    {
        [SerializeField] 
        private PlayerMovementBehaviour playerMovementBehaviour;
        
        [SerializeField] 
        private PlayerVictoryBehaviour playerVictoryBehaviour;
        
        [SerializeField] 
        private Animator playerAnimator;

        [SerializeField] 
        private Transform spriteRendererTransform;

        [SerializeField] 
        private float recordingInterval;
        
        private readonly List<GhostFrame> frames = new();
        private readonly List<GhostEvent> ghostEvents = new();
        private float startTime;
        private float previousFrameTime;
        private float victoryTime;
        private ColourId colourId;

        private void Awake()
        {
            ColourManager.OnColourChangeInstant += HandleColourChangeInstant;
            ColourManager.OnColourChangeStarted += HandleColourChangeStarted;
            DroneTrackerService.OnDroneKilled += HandleDroneKilled;

            playerMovementBehaviour.OnJump += HandleJump;
            playerMovementBehaviour.OnLanded += HandleLanded;
            playerMovementBehaviour.OnPlayerHooked += HandleHooked;
            playerMovementBehaviour.OnPlayerUnhooked += HandleUnhooked;

            playerVictoryBehaviour.OnVictorySequenceStart += HandleVictorySequenceStart;
        }

        private void Start()
        {
            frames.Clear();
            startTime = Time.time;
            previousFrameTime = float.MinValue;
        }

        private void HandleVictorySequenceStart(Vector2 _1, float _2)
        {
            victoryTime = Time.time - startTime;
        }

        private void HandleDroneKilled(DroneHitboxBehaviour drone)
        {
            ghostEvents.Add(new GhostEvent
            {
                type = GhostEventType.DroneKill,
                time = Time.time - startTime,
                data = drone.Id
            });
        }

        private void HandleColourChangeStarted(ColourId colour, float _)
        {
            colourId = colour;
        }

        private void HandleColourChangeInstant(ColourId colour)
        {
            colourId = colour;
        }
        
        private void HandleJump()
        {
            ghostEvents.Add(new GhostEvent
            {
                type = GhostEventType.Jump,
                time = Time.time - startTime
            });
        }

        private void HandleLanded()
        {
            ghostEvents.Add(new GhostEvent
            {
                type = GhostEventType.Land,
                time = Time.time - startTime
            });
        }

        private void HandleHooked()
        {
            ghostEvents.Add(new GhostEvent
            {
                type = GhostEventType.ZiplineHook,
                time = Time.time - startTime
            });
        }

        private void HandleUnhooked()
        {
            ghostEvents.Add(new GhostEvent
            {
                type = GhostEventType.ZiplineUnhook,
                time = Time.time - startTime
            });
        }

        private void Update()
        {
            var currentTime = Time.time - startTime;

            if (currentTime - previousFrameTime < recordingInterval) return;
            
            previousFrameTime = currentTime;

            var trans = transform;
            
            frames.Add(new GhostFrame 
            {
                time = currentTime,
                position = trans.position.xy(),
                zRotation = trans.rotation.eulerAngles.z,
                colourId = colourId,
                animationStateHash = playerAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash,
                isFacingRight = spriteRendererTransform.localScale.x > 0f
            });
        }

        public void SaveGhostData()
        {
            var ghostRun = new GhostRun
            {
                frames = frames,
                droneKills = ghostEvents,
                victoryTime = victoryTime
            };
            
            var bytes = GhostCompressor.Serialize(ghostRun);

            SceneLoader.Instance.CurrentLevelData.SetGhostData(bytes);
        }
        
        private void OnDestroy()
        {
            ColourManager.OnColourChangeInstant -= HandleColourChangeInstant;
            ColourManager.OnColourChangeStarted -= HandleColourChangeStarted;
            
            playerMovementBehaviour.OnJump -= HandleJump;
            playerMovementBehaviour.OnLanded -= HandleLanded;
            playerMovementBehaviour.OnPlayerHooked -= HandleHooked;
            playerMovementBehaviour.OnPlayerUnhooked -= HandleUnhooked;
            
            playerVictoryBehaviour.OnVictorySequenceStart -= HandleVictorySequenceStart;
        }
    }
}