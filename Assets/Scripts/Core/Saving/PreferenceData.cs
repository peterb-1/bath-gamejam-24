using System;
using Cysharp.Threading.Tasks;
using Gameplay.Trails;
using UnityEngine;

namespace Core.Saving
{
    [Serializable]
    public class PreferenceData
    {
        [field: SerializeField] 
        public string TrailGuid { get; private set; }

        public event Action<Trail> OnTrailSet;

        public void SetTrail(Trail trail)
        {
            TrailGuid = trail.Guid;
            
            OnTrailSet?.Invoke(trail);
        }

        public async UniTask InitialiseAsync()
        {
            
        }
    }
}