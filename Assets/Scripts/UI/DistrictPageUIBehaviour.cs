using System;
using Core.Saving;
using Cysharp.Threading.Tasks;
using Gameplay.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class DistrictPageUIBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private int districtNumber;
        
        [SerializeField] 
        private TMP_Text districtNameText;
        
        [SerializeField] 
        private TMP_Text totalCompletedText;
        
        [SerializeField] 
        private TMP_Text totalStarsText;

        [SerializeField] 
        private float linkOffset;
        
        [field: SerializeField]
        public Page Page { get; private set; }
        
        [field: SerializeField] 
        public Button SettingsButton { get; private set; }
        
        [field: SerializeField] 
        public Button LeaderboardButton { get; private set; }

        [field: SerializeField]
        public LevelSelectButton[] LevelSelectButtons { get; private set; }

        public event Action OnSettingsClicked;
        public event Action OnLeaderboardClicked;

        private void Awake()
        {
            CreateButtonLinks();
            SetInfoAsync().Forget();
            
            SettingsButton.onClick.AddListener(HandleSettingsClicked);
            LeaderboardButton.onClick.AddListener(HandleLeaderboardClicked);
        }
        
        private void CreateButtonLinks()
        {
            for (var i = 1; i < LevelSelectButtons.Length; i++)
            {
                var leftButton = LevelSelectButtons[i - 1];
                var rightButton = LevelSelectButtons[i];

                if (leftButton.IsHidden())
                {
                    leftButton = LevelSelectButtons[i - 2];
                }

                var startTarget = leftButton.transform.position;
                var endTarget = rightButton.transform.position;
                var startAnchor = rightButton.IsHidden() ? leftButton.HiddenConnectionAnchor : leftButton.RightConnectionAnchor;
                var endAnchor = rightButton.LeftConnectionAnchor;
                var offset = (endTarget - startTarget).normalized * linkOffset;

                startAnchor.localPosition = offset;
                endAnchor.localPosition = -offset;
                
                rightButton.EnableLink(startAnchor);
            }
        }
        
        private void HandleSettingsClicked()
        {
            OnSettingsClicked?.Invoke();
        }
        
        private void HandleLeaderboardClicked()
        {
            OnLeaderboardClicked?.Invoke();
        }

        private async UniTask SetInfoAsync()
        {
            await UniTask.WaitUntil(() => SaveManager.IsReady);
            
            var (completed, stars, totalMissions) = SaveManager.Instance.SaveData.CampaignData.GetDistrictProgress(districtNumber);

            districtNameText.text = $"{LevelConfig.GetRomanNumeral(districtNumber)}. {LevelConfig.GetDistrictName(districtNumber)}";
            totalCompletedText.text = $"{completed} / {totalMissions}";
            totalStarsText.text = $"{stars} / {3 * totalMissions}";
        }
        
        public void SetTopNavigation(Selectable left, Selectable right)
        {
            var leaderboardNavigation = LeaderboardButton.navigation;
            var settingsNavigation = SettingsButton.navigation;

            leaderboardNavigation.selectOnLeft = left;
            settingsNavigation.selectOnRight = right;

            LeaderboardButton.navigation = leaderboardNavigation;
            SettingsButton.navigation = settingsNavigation;
        }

        public LevelSelectButton GetLeftmostUnlockedLevelButton()
        {
            foreach (var button in LevelSelectButtons)
            {
                if (SaveManager.Instance.SaveData.CampaignData.TryGetLevelData(button.SceneConfig.LevelConfig, out var levelData) &&
                    levelData.IsUnlocked)
                {
                    return button;
                }
            }
            
            return null;
        }
        
        public LevelSelectButton GetRightmostUnlockedLevelButton()
        {
            for (var i = LevelSelectButtons.Length - 1; i >= 0; i--)
            {
                var button = LevelSelectButtons[i];
                
                if (SaveManager.Instance.SaveData.CampaignData.TryGetLevelData(button.SceneConfig.LevelConfig, out var levelData) &&
                    levelData.IsUnlocked)
                {
                    return button;
                }
            }

            return null;
        }

        private void OnDestroy()
        {
            SettingsButton.onClick.RemoveListener(HandleSettingsClicked);
            LeaderboardButton.onClick.RemoveListener(HandleLeaderboardClicked);
        }
    }
}