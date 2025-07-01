using System;
using Core.Saving;
using Gameplay.Trails;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
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
        private float movementAmount;
        
        [SerializeField] 
        private float movementSpeed;

        private TrailRenderer activePreviewTrail;
        private Trail currentTrail;
        
        private bool isCurrentTrailUnlocked;

        private Vector3[] trailPositionsBuffer = new Vector3[64];

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

        private void Update()
        {
            var y = Mathf.Sin(Time.time * movementSpeed) * movementAmount;
            
            trailPreviewParent.transform.localPosition = new Vector3(0f, y, 0f);

            if (activePreviewTrail == null) return;
            
            var positionsCount = activePreviewTrail.positionCount;
            if (positionsCount == 0) return;

            if (trailPositionsBuffer.Length < positionsCount)
            {
                trailPositionsBuffer = new Vector3[positionsCount];
            }

            activePreviewTrail.GetPositions(trailPositionsBuffer);

            var scrollOffset = trailScrollSpeed * Time.deltaTime;

            for (var i = 0; i < positionsCount; i++)
            {
                trailPositionsBuffer[i] += Vector3.left * scrollOffset;
            }

            activePreviewTrail.SetPositions(trailPositionsBuffer);
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
            isCurrentTrailUnlocked = trail.IsUnlockedByDefault || SaveManager.Instance.SaveData.AchievementsData.IsAchievementWithTrailUnlocked(trail);
            trailNameText.text = trail.Name;

            UpdateTrailPreview(trail);
        }
        
        public void EmitTrail()
        {
            if (activePreviewTrail != null)
            {
                activePreviewTrail.Clear();
                activePreviewTrail.emitting = true;
            }
        }

        public void StopEmitting()
        {
            if (activePreviewTrail != null)
            {
                activePreviewTrail.emitting = false;
            }
        }

        private void UpdateTrailPreview(Trail trail)
        {
            if (trailPreviewParent == null || trail.TrailRenderer == null) return;

            var trailInstance = Instantiate(trail.TrailRenderer, trailPreviewParent);
            
            trailInstance.Clear();
            trailInstance.emitting = true;
            trailInstance.sortingLayerName = "UI";
            trailInstance.sortingOrder = 1;
            trailInstance.colorGradient = trailInstance.colorGradient.WithTint(new Color(0.8f, 0.8f, 0.8f));

            activePreviewTrail = trailInstance;
        }
        
        private void OnDestroy()
        {
            button.onClick.RemoveListener(HandleButtonClicked);
            
            button.OnHover -= HandleHover;
            button.OnUnhover -= HandleUnhover;
        }
    }
}
