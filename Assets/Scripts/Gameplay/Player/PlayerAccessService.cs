using System;
using UnityEngine;
using Utils;

namespace Gameplay.Player
{
    public class PlayerAccessService : MonoBehaviour
    {
        [field: SerializeField]
        public Transform PlayerTransform { get; private set; }
        
        [field: SerializeField]
        public PlayerMovementBehaviour PlayerMovementBehaviour { get; private set; }
    
        [field: SerializeField]
        public PlayerColourBehaviour PlayerColourBehaviour { get; private set; }
        
        public static PlayerAccessService Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                GameLogger.LogError("Cannot have more than one PlayerAccessService in the scene at once! Destroying this one.");
                Destroy(this);
            }

            Instance = this;
        }

        public static bool IsReady() => Instance != null;

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}
