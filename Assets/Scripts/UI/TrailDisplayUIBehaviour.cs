using System;
using Core.Saving;
using Gameplay.Trails;
using TMPro;
using UnityEngine;

namespace UI
{
    public class TrailDisplayUIBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private ExtendedButton button;

        [SerializeField] 
        private TMP_Text trailNameText;

        private Trail currentTrail;
        private bool isCurrentTrailUnlocked;

        public event Action<Trail> OnTrailSelected;
        public event Action<Trail> OnTrailHovered;
        public event Action<Trail> OnTrailUnhovered;

        private void Awake()
        {
            button.onClick.AddListener(HandleButtonClicked);
            
            button.OnHover += HandleHover;
            button.OnUnhover += HandleUnhover;
        }

        private void HandleButtonClicked()
        {
            if (isCurrentTrailUnlocked)
            {
                OnTrailSelected?.Invoke(currentTrail);
            }
        }

        private void HandleHover(ExtendedButton _)
        {
            OnTrailHovered?.Invoke(currentTrail);
        }

        private void HandleUnhover(ExtendedButton _)
        {
            OnTrailUnhovered?.Invoke(currentTrail);
        }

        public void DisplayTrailInfo(Trail trail)
        {
            currentTrail = trail;
            isCurrentTrailUnlocked = trail.IsUnlockedByDefault || 
                                     SaveManager.Instance.SaveData.AchievementsData.IsAchievementWithTrailUnlocked(trail);

            trailNameText.text = trail.Name;

            button.interactable = isCurrentTrailUnlocked;
        }
        
        private void OnDestroy()
        {
            button.onClick.RemoveListener(HandleButtonClicked);
            
            button.OnHover -= HandleHover;
            button.OnUnhover -= HandleUnhover;
        }
    }
}