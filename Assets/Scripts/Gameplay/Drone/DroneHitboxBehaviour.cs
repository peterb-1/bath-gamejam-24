using System;
using System.Collections.Generic;
using Audio;
using Core.Saving;
using Cysharp.Threading.Tasks;
using Gameplay.Player;
using NaughtyAttributes;
using UnityEngine;
using Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gameplay.Drone
{
    public class DroneHitboxBehaviour : MonoBehaviour
    {
        [field: SerializeField, ReadOnly]
        public ushort Id { get; private set; }

        [SerializeField] 
        private Animator droneAnimator;
        
        [SerializeField]
        private Collider2D droneCollider;
    
        [SerializeField] 
        private Rigidbody2D rigidBody;
        
        [SerializeField] 
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private LayerMask playerLayers;

        [SerializeField] 
        private Vector2 headAngleRange;

        [SerializeField] 
        private Vector2 deathDirectionStrength;

        [SerializeField] 
        private AnimationCurve fadeCurve;

        [SerializeField] 
        private float fadeDuration;
        
        [field: SerializeField] 
        public float TimeBonusOnKilled { get; private set; }

        private PlayerMovementBehaviour playerMovementBehaviour;
        private PlayerDeathBehaviour playerDeathBehaviour;
        
        private static readonly int Died = Animator.StringToHash("died");
        private static readonly int Threshold = Shader.PropertyToID("_Threshold");

        public event Action<DroneHitboxBehaviour> OnDroneKilled;
        public event Action<DroneHitboxBehaviour> OnDroneKilledByGhost;

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerMovementBehaviour = PlayerAccessService.Instance.PlayerMovementBehaviour;
            playerDeathBehaviour = PlayerAccessService.Instance.PlayerDeathBehaviour;
            
            DroneTrackerService.RegisterDrone(this);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((playerLayers.value & (1 << other.gameObject.layer)) != 0)
            {
                var direction = other.bounds.center.xy() - droneCollider.bounds.center.xy();
                var angle = Vector2.SignedAngle(transform.right.xy(), direction);

                if (angle > headAngleRange.x && angle < headAngleRange.y)
                {
                    HandlePlayerKilledDrone(direction);
                }
                else
                {
                    HandleDroneKilledPlayer();
                }
            }
        }

        private void HandlePlayerKilledDrone(Vector2 direction)
        {
            OnDroneKilled?.Invoke(this);
            
            playerMovementBehaviour.PerformHeadJump();
            
            SaveManager.Instance.SaveData.StatsData.AddToStat(StatType.DronesKilled, 1);

            FallAway(direction);
        }

        public void NotifyKilledByGhost(Vector2 ghostPosition)
        {
            OnDroneKilledByGhost?.Invoke(this);
            
            var direction = ghostPosition - droneCollider.bounds.center.xy();
            
            FallAway(direction);
        }

        private void FallAway(Vector2 direction)
        {
            AudioManager.Instance.Play(AudioClipIdentifier.DroneDeath);
            
            direction.Normalize();
            
            rigidBody.simulated = true;
            rigidBody.linearVelocity = -direction * deathDirectionStrength;
            rigidBody.gravityScale = 1f;
            rigidBody.constraints = RigidbodyConstraints2D.None;
            
            droneCollider.enabled = false;
            droneAnimator.SetTrigger(Died);
            
            SetActive(false, shouldAnimate: true);
        }
        
        private void HandleDroneKilledPlayer()
        {
            playerDeathBehaviour.KillPlayer(PlayerDeathSource.Drone);
        }

        public void SetActive(bool isActive, bool shouldAnimate = false)
        {
            if (shouldAnimate)
            {
                AnimateActiveAsync(isActive).Forget();
            }
            else
            {
                spriteRenderer.material.SetFloat(Threshold, isActive ? 0f : 1f);
            }
        }
        
        private async UniTask AnimateActiveAsync(bool isActive)
        {
            var timeElapsed = 0f;

            while (timeElapsed < fadeDuration)
            {
                var lerp = fadeCurve.Evaluate(timeElapsed / fadeDuration);

                spriteRenderer.material.SetFloat(Threshold, isActive ? 1f - lerp : lerp);
                
                await UniTask.Yield();

                timeElapsed += Time.deltaTime;
            }
            
            spriteRenderer.material.SetFloat(Threshold, isActive ? 0f : 1f);

            if (!isActive)
            {
                rigidBody.simulated = false;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResetIds(false);
        }
        
        [Button("Force Reset IDs")]
        private void ForceResetIds()
        {
            ResetIds(true);
        }

        private void ResetIds(bool forceReset)
        {
            if (EditorApplication.isPlaying) return;
            
            var allDrones = FindObjectsByType<DroneHitboxBehaviour>(FindObjectsSortMode.None);
            var resetCount = 0;

            var assignedIds = new HashSet<ushort>();
            var rand = new System.Random();

            foreach (var drone in allDrones)
            {
                if (PrefabUtility.IsPartOfPrefabAsset(drone)) continue;
                
                if (drone.Id != 0 && !forceReset)
                {
                    if (assignedIds.Add(drone.Id))
                    {
                        continue;
                    }
                }
                
                ushort newId;
                do
                {
                    newId = (ushort) rand.Next(1, ushort.MaxValue);
                } while (!assignedIds.Add(newId));

                drone.Id = newId;
                
                EditorUtility.SetDirty(drone);

                resetCount++;
            }

            GameLogger.Log($"Reset IDs for {resetCount} drones.");
        }
#endif
    }
}