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

        public void SetTrail(Trail trail)
        {
            TrailGuid = trail.Guid;
        }

        public async UniTask InitialiseAsync()
        {
            
        }
    }
}