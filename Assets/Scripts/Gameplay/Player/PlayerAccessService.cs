using Gameplay.Ghosts;
using UI;
using UnityEngine;
using Utils;

namespace Gameplay.Player
{
    public class PlayerAccessService : MonoBehaviour
    {
        [field: SerializeField]
        public Transform PlayerTransform { get; private set; }
        
        [field: SerializeField]
        public PlayerColourBehaviour PlayerColourBehaviour { get; private set; }
        
        [field: SerializeField]
        public PlayerDeathBehaviour PlayerDeathBehaviour { get; private set; }
        
        [field: SerializeField]
        public PlayerMovementBehaviour PlayerMovementBehaviour { get; private set; }
        
        [field: SerializeField]
        public PlayerVictoryBehaviour PlayerVictoryBehaviour { get; private set; }
        
        [field: SerializeField]
        public PlayerTrailBehaviour PlayerTrailBehaviour { get; private set; }
        
        [field: SerializeField]
        public TimerBehaviour TimerBehaviour { get; private set; }
        
        [field: SerializeField]
        public GhostWriter GhostWriter { get; private set; }

        public static PlayerAccessService Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                GameLogger.LogError("Cannot have more than one PlayerAccessService in the scene at once! Destroying this one.");
                Destroy(this);
                return;
            }

            Instance = this;
        }

        public static bool IsReady() => Instance != null;

        public void DisablePlayerBehavioursForSpectate()
        {
            PlayerMovementBehaviour.DisableForSpectate();

            for (var i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}
