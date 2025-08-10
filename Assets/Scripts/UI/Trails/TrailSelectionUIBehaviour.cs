using System.Collections.Generic;
using Core.Saving;
using Cysharp.Threading.Tasks;
using Gameplay.Trails;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Trails
{
    public class TrailSelectionUIBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private Button tabButton;
        
        [SerializeField] 
        private TrailDatabase trailDatabase;
        
        [SerializeField] 
        private GridLayoutGroup gridLayout;
        
        [SerializeField] 
        private RectTransform gridTransform;
        
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
                var trailDisplay = Instantiate(trailDisplayPrefab, gridLayout.transform);
                
                trailDisplay.DisplayTrailInfo(trail);

                trailDisplay.OnTrailSelected += HandleTrailSelected;
                trailDisplay.OnTrailHovered += HandleTrailHovered;
                trailDisplay.OnTrailUnhovered += HandleTrailUnhovered;
                
                spawnedTrailDisplays.Add(trailDisplay);
            }

            customisationPage.OnShown += HandlePageShown;
            customisationPage.OnHidden += HandlePageHidden;
            
            HandlePageHidden();
            SetSelectedTrailInfo();
            SetNavigation();
        }

        private void HandleTrailSelected(Trail trail)
        {
            SaveManager.Instance.SaveData.PreferenceData.SetValue(SettingId.Trail, trail.Guid);
            SaveManager.Instance.Save();

            SetSelectedTrailInfo();
        }

        private void SetSelectedTrailInfo()
        {
            if (SaveManager.Instance.SaveData.PreferenceData.TryGetValue(SettingId.Trail, out string trailGuid))
            {
                foreach (var trailDisplay in spawnedTrailDisplays)
                {
                    trailDisplay.NotifySelectedTrailGuid(trailGuid);
                }
            }
        }
        
        private void SetNavigation()
        {
            var columns = gridLayout.GetColumnCount(gridTransform);

            for (var i = 0; i < spawnedTrailDisplays.Count; i++)
            {
                var upperSelectable = i < columns 
                    ? tabButton 
                    : spawnedTrailDisplays[i - columns].Button;
                
                var lowerSelectable = i + columns >= spawnedTrailDisplays.Count
                    ? null
                    : spawnedTrailDisplays[i + columns].Button;
                
                var leftSelectable = i % columns == 0 
                    ? null 
                    : spawnedTrailDisplays[i - 1].Button;
                
                var rightSelectable = i + 1 >= spawnedTrailDisplays.Count || (i + 1) % columns == 0 
                    ? null 
                    : spawnedTrailDisplays[i + 1].Button;

                var navigation = spawnedTrailDisplays[i].Button.navigation;

                navigation.selectOnUp = upperSelectable;
                navigation.selectOnDown = lowerSelectable;
                navigation.selectOnLeft = leftSelectable;
                navigation.selectOnRight = rightSelectable;

                spawnedTrailDisplays[i].Button.navigation = navigation;
            }
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