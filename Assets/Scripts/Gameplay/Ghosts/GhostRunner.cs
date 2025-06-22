using System.Collections.Generic;
using Core;
using Core.Saving;
using Cysharp.Threading.Tasks;
using Gameplay.Colour;
using Gameplay.Core;
using UnityEngine;
using Utils;

namespace Gameplay.Ghosts
{
    public class GhostRunner : MonoBehaviour
    {
        public const string GHOST_DATA_KEY = "GhostData";
        
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
        private ColourId currentColour;

        private float playbackTime;
        private float victoryTime;
        private int currentAnimatorStateHash;
        private int currentIndex;
        private bool isFinishing;
        
        private void Awake()
        {
            GhostRun ghostRun = null;

            if (SceneLoader.Instance.SceneLoadContext != null &&
                SceneLoader.Instance.SceneLoadContext.TryGetCustomData(GHOST_DATA_KEY, out GhostContext ghostContext))
            {
                ghostRun = ghostContext.GhostRun;
            }

            if (ghostRun == null &&
                SaveManager.Instance.SaveData.CampaignData.TryGetLevelData(SceneLoader.Instance.CurrentSceneConfig.LevelConfig, out var levelData) &&
                levelData is { GhostData: not null })
            {
                ghostRun = GhostCompressor.Deserialize(levelData.GhostData);
            }

            if (ghostRun == null)
            {
                Destroy(gameObject);
            }
            else
            {
                frames = ghostRun.frames;
                victoryTime = ghostRun.victoryTime;
                
                if (frames is { Count: > 0 } && colourDatabase.TryGetColourConfig(frames[0].colourId, out var colourConfig))
                {
                    spriteRenderer.color = colourConfig.PlayerColour;
                }
            }
        }

        private void Update() 
        {
            if (frames == null || frames.Count == 0 || PauseManager.Instance.IsPaused) return;

            // increment to current frame
            playbackTime += Time.deltaTime;
            playbackTime = Mathf.Min(playbackTime, frames[^1].time);

            while (currentIndex < frames.Count - 1 && frames[currentIndex + 1].time <= playbackTime) 
            {
                currentIndex++;
            }

            // if we've reached the end of the ghost
            if (currentIndex >= frames.Count - 1) 
            {
                Destroy(gameObject);

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
            }

            spriteRenderer.flipX = lerp < 0.5f ? !frameA.isFacingRight : !frameB.isFacingRight;
            
            // check if we need to shrink the player as they approach the end
            if (playbackTime > victoryTime && !isFinishing)
            {
                isFinishing = true;
                
                RunVictoryShrinkAsync().Forget();
            }
        }

        private async UniTask RunVictoryShrinkAsync()
        {
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