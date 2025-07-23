using System.Collections.Generic;
using Core.Saving;
using Cysharp.Threading.Tasks;
using Gameplay.Trails;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Trails
{
    public class TrailSelectionUIBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private TrailDatabase trailDatabase;
        
        [SerializeField] 
        private Transform trailGridRoot;
        
        [SerializeField] 
        private TrailDisplayUIBehaviour trailDisplayPrefab;

        [SerializeField] 
        private SelectedTrailDisplayUIBehaviour selectedTrailDisplay;

        [SerializeField] 
        private Page customisationPage;

        private readonly List<TrailDisplayUIBehaviour> spawnedTrailDisplays = new();

        public Selectable FirstSelectable => spawnedTrailDisplays[0].Button;

        private async void Awake()
        {
            await UniTask.WaitUntil(() => SaveManager.IsReady);
            
            foreach (var trail in trailDatabase.Trails)
            {
                var trailDisplay = Instantiate(trailDisplayPrefab, trailGridRoot);
                
                trailDisplay.DisplayTrailInfo(trail);

                trailDisplay.OnTrailSelected += HandleTrailSelected;
                trailDisplay.OnTrailHovered += HandleTrailHovered;
                trailDisplay.OnTrailUnhovered += HandleTrailUnhovered;
                
                spawnedTrailDisplays.Add(trailDisplay);
            }

            customisationPage.OnShown += HandlePageShown;
            customisationPage.OnHidden += HandlePageHidden;
            
            HandlePageHidden();
        }

        private void HandleTrailSelected(Trail trail)
        {
            SaveManager.Instance.SaveData.PreferenceData.SetValue(SettingId.Trail, trail.Guid);
            SaveManager.Instance.Save();
        }

        private void HandleTrailHovered(Trail trail)
        {
            selectedTrailDisplay.SetTrailInfo(trail);
        }

        private void HandleTrailUnhovered(Trail trail)
        {
            selectedTrailDisplay.SetNoData();
        }
        
        private void HandlePageShown()
        {
            foreach (var trailDisplay in spawnedTrailDisplays)
            {
                trailDisplay.EmitTrail();
            }
        }

        private void HandlePageHidden()
        {
            foreach (var trailDisplay in spawnedTrailDisplays)
            {
                trailDisplay.StopEmitting();
            }
        }

        private void OnDestroy()
        {
            foreach (var trailDisplay in spawnedTrailDisplays)
            {
                trailDisplay.OnTrailSelected -= HandleTrailSelected;
                trailDisplay.OnTrailHovered -= HandleTrailHovered;
                trailDisplay.OnTrailUnhovered -= HandleTrailUnhovered;
            }
            
            customisationPage.OnShown -= HandlePageShown;
            customisationPage.OnHidden -= HandlePageHidden;
            
            spawnedTrailDisplays.Clear();
        }
    }
}