using System;
using System.Collections.Generic;
using Audio;
using Cysharp.Threading.Tasks;
using Gameplay.Input;
using Gameplay.Player;
using UnityEngine;
using Utils;

namespace Gameplay.Dash
{
    public class DashTrackerService : MonoBehaviour
    {
        [SerializeField] 
        private int maxOrbCapacity;

        private PlayerMovementBehaviour playerMovementBehaviour;
        private int currentOrbs;
        
        private static readonly HashSet<DashOrb> CollectedOrbs = new();
        
        public static DashTrackerService Instance { get; private set; }
        
        public static event Action<int> OnDashGained;
        public static event Action<int> OnDashUsed;
        public static event Action OnDashFailed;
        
        private async void Awake()
        {
            if (Instance != null && Instance != this)
            {
                GameLogger.LogError("Cannot have more than one DashTrackerService in the scene at once! Destroying this one.");
                Destroy(this);
                return;
            }

            Instance = this;

            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            InputManager.OnDashPerformed += HandleDashPerformed;
            
            playerMovementBehaviour = PlayerAccessService.Instance.PlayerMovementBehaviour;
        }

        private void HandleDashPerformed()
        {
            if (currentOrbs <= 0 || playerMovementBehaviour.IsDashing || playerMovementBehaviour.IsHooked)
            {
                AudioManager.Instance.Play(AudioClipIdentifier.DashFailed);
                
                OnDashFailed?.Invoke();
            }
            else
            {
                currentOrbs--;
                
                AudioManager.Instance.Play(AudioClipIdentifier.Dash);
                
                OnDashUsed?.Invoke(currentOrbs);
            }
        }

        public bool TryCollect(DashOrb dashOrb)
        {
            if (currentOrbs >= maxOrbCapacity || CollectedOrbs.Contains(dashOrb))
            {
                return false;
            }
            
            AudioManager.Instance.Play(AudioClipIdentifier.DashCollected);
            
            CollectedOrbs.Add(dashOrb);
            currentOrbs++;

            OnDashGained?.Invoke(currentOrbs);
            
            return true;
        }
        
        private void OnDestroy()
        {
            InputManager.OnDashPerformed -= HandleDashPerformed;
            
            Instance = null;
        }
    }
}