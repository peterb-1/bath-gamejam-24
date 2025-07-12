using System;
using System.Collections.Generic;
using Audio;
using Core;
using Core.Saving;
using Cysharp.Threading.Tasks;
using Gameplay.Input;
using Gameplay.Player;
using UnityEngine;
using Utils;

namespace Gameplay.Dash
{
    public class DashTrackerService : MonoBehaviour
    {
        public const int DASH_INTRODUCTION_DISTRICT = 4;
        
        [SerializeField] 
        private int maxOrbCapacity;

        private readonly HashSet<DashOrb> collectedOrbs = new();
        private PlayerMovementBehaviour playerMovementBehaviour;
        private int currentOrbs;
        
        public bool HasFullOrbCapacity => currentOrbs == maxOrbCapacity;
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

            await UniTask.WaitUntil(SceneLoader.IsReady);
            
            if (SceneLoader.Instance.CurrentSceneConfig.LevelConfig.DistrictNumber < DASH_INTRODUCTION_DISTRICT) return;

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
                
                SaveManager.Instance.SaveData.StatsData.AddToStat(StatType.DashesMade, 1);
            }
        }

        public bool TryCollect(DashOrb dashOrb)
        {
            if (currentOrbs >= maxOrbCapacity || collectedOrbs.Contains(dashOrb))
            {
                return false;
            }
            
            AudioManager.Instance.Play(AudioClipIdentifier.DashCollected);
            
            collectedOrbs.Add(dashOrb);
            currentOrbs++;

            OnDashGained?.Invoke(currentOrbs);
            
            return true;
        }
        
        public void TryCollectFromSpectatorGhost(ushort orbId)
        {
            
        }

        private void OnDestroy()
        {
            InputManager.OnDashPerformed -= HandleDashPerformed;
            
            Instance = null;
        }
    }
}