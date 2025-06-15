using System;
using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Core;
using Gameplay.Ghosts;
using Steam;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class LeaderboardRow : MonoBehaviour
    {
        [SerializeField] 
        private TMP_Text positionText;
        
        [SerializeField] 
        private TMP_Text usernameText;
        
        [SerializeField] 
        private TMP_Text timeText;
        
        [SerializeField] 
        private RankingStarUIBehaviour rankingStarUIBehaviour;

        [SerializeField] 
        private Button ghostButton;

        private SceneConfig currentSceneConfig;
        private int[] entryDetails;

        private void Awake()
        {
            ghostButton.onClick.AddListener(HandleGhostButtonClicked);
        }

        private void HandleGhostButtonClicked()
        {
            GetGhostDataAsync().Forget();
        }

        private async UniTask GetGhostDataAsync()
        {
            var ghostData = await SteamLeaderboards.Instance.TryGetGhostDataAsync(currentSceneConfig.LevelConfig, entryDetails);
            var sceneLoadContext = new CustomDataContainer();
            
            sceneLoadContext.SetCustomData(GhostRunner.GHOST_DATA_KEY, ghostData);
            
            SceneLoader.Instance.LoadScene(currentSceneConfig, sceneLoadContext);
        }

        public void SetDetails(int position, string username, float time, int[] details, SceneConfig sceneConfig)
        {
            positionText.text = $"{position}";
            usernameText.text = username;
            timeText.text = TimerBehaviour.GetFormattedTime(time);
            rankingStarUIBehaviour.SetRanking(sceneConfig.LevelConfig.GetTimeRanking(time));

            entryDetails = details;
            currentSceneConfig = sceneConfig;
        }

        private void OnDestroy()
        {
            ghostButton.onClick.RemoveListener(HandleGhostButtonClicked);
        }
    }
}