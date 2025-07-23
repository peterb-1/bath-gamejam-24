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
        
        private AbstractGameplayTrailBehaviour gameplayTrailBehaviour;

        public event Action<AbstractGameplayTrailBehaviour> OnTrailLoaded;
        
        private async void Awake()
        {
            await UniTask.WaitUntil(() => SaveManager.IsReady);

            SaveManager.Instance.SaveData.PreferenceData.TryGetValue(SettingId.Trail, out string preferredTrailGuid);

            if (!trailDatabase.TryGetTrail(preferredTrailGuid, out var trail))
            {
                if (!trailDatabase.TryGetTrail(trailDatabase.DefaultTrail, out trail))
                {
                    GameLogger.LogError("Cannot get player trail!", this);
                    return;
                }
                
                SaveManager.Instance.SaveData.PreferenceData.SetValue(SettingId.Trail, trail.Guid);
            }

            if (trail.GameplayTrailBehaviour == null) return;
            
            gameplayTrailBehaviour = Instantiate(trail.GameplayTrailBehaviour, transform, false);
            
            OnTrailLoaded?.Invoke(gameplayTrailBehaviour);
        }
    }
}