using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gameplay.Dash
{
    public class DashOrb : MonoBehaviour
    {
        [field: SerializeField, ReadOnly]
        public ushort Id { get; private set; }
        
        [SerializeField] 
        private SpriteRenderer spriteRenderer;

        [SerializeField] 
        private Collider2D collectionCollider;
        
        [SerializeField] 
        private LayerMask playerLayers;
        
        [SerializeField] 
        private AnimationCurve dissolveCurve;

        [SerializeField] 
        private float dissolveDuration;
        
        private static readonly int Threshold = Shader.PropertyToID("_Threshold");
        
        private async void Awake()
        {
            await UniTask.WaitUntil(DashTrackerService.IsReady);
            
            DashTrackerService.Instance.RegisterDash(this);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((playerLayers.value & (1 << other.gameObject.layer)) != 0)
            {
                if (DashTrackerService.Instance.TryCollect(this))
                {
                    collectionCollider.enabled = false;
                    
                    RunDissolveAsync().Forget();
                }
            }
        }
        
        public void NotifyCollectedByGhost()
        {
            RunDissolveAsync().Forget();
        }
        
        private async UniTask RunDissolveAsync()
        {
            var timeElapsed = 0f;

            while (timeElapsed < dissolveDuration)
            {
                var lerp = dissolveCurve.Evaluate(timeElapsed / dissolveDuration);

                spriteRenderer.material.SetFloat(Threshold, lerp);
                
                await UniTask.Yield();

                timeElapsed += Time.deltaTime;
            }
            
            spriteRenderer.material.SetFloat(Threshold, dissolveCurve.Evaluate(1f));
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            ResetIds();
        }

        [Button("Reset IDs")]
        private void ResetIds()
        {
            if (EditorApplication.isPlaying) return;
            
            var allOrbs = FindObjectsByType<DashOrb>(FindObjectsSortMode.None);

            var assignedIds = new HashSet<ushort>();
            var rand = new System.Random();

            foreach (var dashOrb in allOrbs)
            {
                ushort newId;
                do
                {
                    newId = (ushort)rand.Next(1, ushort.MaxValue);
                } while (!assignedIds.Add(newId));

                dashOrb.Id = newId;
                
                EditorUtility.SetDirty(dashOrb);
            }

            GameLogger.Log($"Reset IDs for {allOrbs.Length} orbs.");
        }
#endif
    }
}