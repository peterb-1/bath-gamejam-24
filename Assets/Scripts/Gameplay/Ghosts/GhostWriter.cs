using System.Collections.Generic;
using Core;
using Core.Saving;
using Gameplay.Colour;
using Gameplay.Player;
using UnityEngine;
using Utils;

namespace Gameplay.Ghosts
{
    public class GhostWriter : MonoBehaviour
    {
        [SerializeField] 
        private PlayerVictoryBehaviour playerVictoryBehaviour;
        
        [SerializeField] 
        private Animator playerAnimator;

        [SerializeField] 
        private Transform spriteRendererTransform;

        [SerializeField] 
        private float recordingInterval;
        
        private readonly List<GhostFrame> frames = new();
        private float startTime;
        private float previousFrameTime;
        private float victoryTime;
        private ColourId colourId;

        private void Awake()
        {
            ColourManager.OnColourChangeInstant += HandleColourChangeInstant;
            ColourManager.OnColourChangeStarted += HandleColourChangeStarted;

            playerVictoryBehaviour.OnVictorySequenceStart += HandleVictorySequenceStart;
        }

        private void HandleVictorySequenceStart(Vector2 _1, float _2)
        {
            victoryTime = Time.time - startTime;
        }

        private void Start()
        {
            frames.Clear();
            startTime = Time.time;
            previousFrameTime = float.MinValue;
        }

        private void HandleColourChangeStarted(ColourId colour, float _)
        {
            colourId = colour;
        }

        private void HandleColourChangeInstant(ColourId colour)
        {
            colourId = colour;
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
                victoryTime = victoryTime
            };
            
            var bytes = GhostCompressor.Serialize(ghostRun);

            if (SaveManager.Instance.SaveData.CampaignData.TryGetLevelData(SceneLoader.Instance.CurrentSceneConfig.LevelConfig, out var levelData))
            {
                levelData.SetGhostData(bytes);
            }
        }
        
        private void OnDestroy()
        {
            ColourManager.OnColourChangeInstant -= HandleColourChangeInstant;
            ColourManager.OnColourChangeStarted -= HandleColourChangeStarted;
            
            playerVictoryBehaviour.OnVictorySequenceStart -= HandleVictorySequenceStart;
        }
    }
}