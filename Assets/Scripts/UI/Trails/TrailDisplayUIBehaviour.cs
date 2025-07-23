using System;
using Core.Saving;
using Gameplay.Trails;
using TMPro;
using UnityEngine;

namespace UI.Trails
{
    public class TrailDisplayUIBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private ExtendedButton button;

        [SerializeField] 
        private TMP_Text trailNameText;

        [SerializeField]
        private Transform trailPreviewParent;

        [SerializeField] 
        private float trailScrollSpeed;
        
        [SerializeField] 
        private float trailLifetime;
        
        [SerializeField] 
        private float particleScrollSpeed;
        
        [SerializeField] 
        private float particleOffset;

        [SerializeField] 
        private float movementAmount;
        
        [SerializeField] 
        private float movementSpeed;

        private ITrailDisplayStrategy trailDisplayStrategy;
        private Trail currentTrail;
        
        private bool isCurrentTrailUnlocked;

        public ExtendedButton Button => button;

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

        private void Update()
        {
            var y = Mathf.Sin(Time.time * movementSpeed) * movementAmount;
            
            trailPreviewParent.localPosition = new Vector3(0f, y, 0f);
            
            trailDisplayStrategy?.Update();
        }

        public void DisplayTrailInfo(Trail trail)
        {
            currentTrail = trail;
            isCurrentTrailUnlocked = trail.IsUnlockedByDefault || SaveManager.Instance.SaveData.AchievementsData.IsAchievementWithTrailUnlocked(trail);
            trailNameText.text = trail.Name;

            UpdateTrailPreview(trail);
        }
        
        public void EmitTrail()
        {
            trailDisplayStrategy?.EmitTrail();
        }

        public void StopEmitting()
        {
            trailDisplayStrategy?.StopEmitting();
        }

        private void UpdateTrailPreview(Trail trail)
        {
            if (trailPreviewParent == null || trail.GameplayTrailBehaviour == null) return;
            
            switch (trail.GameplayTrailBehaviour)
            {
                case GameplayTrailRendererBehaviour trailRendererBehaviour:
                    var trailRendererBehaviourInstance = Instantiate(trailRendererBehaviour, trailPreviewParent);
                    trailDisplayStrategy = new TrailRendererDisplayStrategy(trailRendererBehaviourInstance.TrailRenderer, trailScrollSpeed, trailLifetime);
                    break;
                case GameplayParticleTrailBehaviour particleTrailBehaviour:
                    var particleTrailBehaviourInstance = Instantiate(particleTrailBehaviour, trailPreviewParent);
                    particleTrailBehaviourInstance.transform.localPosition = new Vector3(particleOffset, 0f, 0f);
                    trailDisplayStrategy = new ParticleTrailDisplayStrategy(particleTrailBehaviourInstance.TrailParticles, particleScrollSpeed, trailLifetime);
                    break;
            }
        }
        
        private void OnDestroy()
        {
            button.onClick.RemoveListener(HandleButtonClicked);
            
            button.OnHover -= HandleHover;
            button.OnUnhover -= HandleUnhover;
        }
    }
}
