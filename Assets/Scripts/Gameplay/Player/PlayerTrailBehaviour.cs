using System;
using Core.Saving;
using Cysharp.Threading.Tasks;
using Gameplay.Trails;
using UnityEngine;
using Utils;

namespace Gameplay.Player
{
    public class PlayerTrailBehaviour : MonoBehaviour
    {
        [SerializeField]
        private TrailDatabase trailDatabase;

        public event Action<TrailRenderer> OnTrailLoaded;
        
        private async void Awake()
        {
            await UniTask.WaitUntil(() => SaveManager.IsReady);

            var preferredTrailGuid = SaveManager.Instance.SaveData.PreferenceData.TrailGuid;

            if (!trailDatabase.TryGetTrail(preferredTrailGuid, out var trail))
            {
                if (!trailDatabase.TryGetTrail(trailDatabase.DefaultTrail, out trail))
                {
                    GameLogger.LogError("Cannot get player trail!", this);
                }
                
                SaveManager.Instance.SaveData.PreferenceData.SetTrail(trail);
            }

            var trailRenderer = Instantiate(trail.TrailRenderer, transform, false);
            
            OnTrailLoaded?.Invoke(trailRenderer);
        }
    }
}