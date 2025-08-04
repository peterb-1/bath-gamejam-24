using System;
using Audio;
using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Core;
using Gameplay.Ghosts;
using Gameplay.Player;
using Hardware;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class SpectateVictoryUIBehaviour : MonoBehaviour
    {
        public const string LEADERBOARD_NAME_KEY = "LeaderboardName";

        [Header("References")]
        [SerializeField] 
        private PageGroup victoryPageGroup;

        [SerializeField] 
        private Button retryButton;
        
        [SerializeField] 
        private Button raceButton;

        [SerializeField] 
        private Button quitButton;

        [SerializeField] 
        private TMP_Text timerText;
        
        [SerializeField] 
        private TMP_Text levelInfoText;
        
        [SerializeField] 
        private TMP_Text spectateInfoText;

        [SerializeField] 
        private TMP_Text oneStarText;
        
        [SerializeField] 
        private TMP_Text twoStarsText;
        
        [SerializeField] 
        private TMP_Text threeStarsText;

        [SerializeField] 
        private TMP_Text rainbowText;
        
        [SerializeField] 
        private TMP_Text oldBestText;

        [SerializeField] 
        private Animator oldBestAnimator;

        [SerializeField] 
        private RankingStar firstStar;
        
        [SerializeField] 
        private RankingStar secondStar;
        
        [SerializeField] 
        private RankingStar thirdStar;

        [Header("Config")]
        [SerializeField] 
        private RumbleConfig starRumbleConfig;
        
        [SerializeField] 
        private RumbleConfig rainbowRumbleConfig;

        [SerializeField] 
        private float starAnimationDelay;
        
        [SerializeField] 
        private float rainbowAnimationDelay;

        [SerializeField] 
        private GameObjectStateSetter rainbowVisibilitySetter;

        private PlayerVictoryBehaviour playerVictoryBehaviour;
        
        private static readonly int Show = Animator.StringToHash("Show");

        private void Awake()
        {
            GhostRunner.OnSpectateVictorySequenceFinish += HandleSpectateVictorySequenceFinish;
            
            retryButton.onClick.AddListener(HandleRetryClicked);
            raceButton.onClick.AddListener(HandleRaceClicked);
            quitButton.onClick.AddListener(HandleQuitClicked);
        }

        private void HandleRetryClicked()
        {
            victoryPageGroup.HideGroup();
            SceneLoader.Instance.ReloadCurrentScene();
        }
        
        private void HandleRaceClicked()
        {
            victoryPageGroup.HideGroup();

            var context = SceneLoader.Instance.SceneLoadContext;
            context.SetCustomData(GhostRunner.SPECTATE_KEY, false);
            context.RemoveCustomData<string>(LEADERBOARD_NAME_KEY);
            
            SceneLoader.Instance.LoadScene(SceneLoader.Instance.CurrentSceneConfig, context);
        }
        
        private void HandleQuitClicked()
        {
            victoryPageGroup.HideGroup();
            SceneLoader.Instance.LoadLevelSelect();
        }

        private async void HandleSpectateVictorySequenceFinish(int finalMilliseconds)
        {
            SetInfoText();
            
            timerText.text = TimerBehaviour.GetFormattedTime(finalMilliseconds);
            
            var ranking = ProcessLevelCompletion(finalMilliseconds);
            
            await victoryPageGroup.ShowGroupAsync();
            await DisplayRankingAsync(ranking);
        }
        
        private void SetInfoText()
        {
            var sceneConfig = SceneLoader.Instance.CurrentSceneConfig;
            var context = SceneLoader.Instance.SceneLoadContext;

            if (sceneConfig.IsLevelScene)
            {
                levelInfoText.text = sceneConfig.LevelConfig.GetLevelText();
            }
            else
            {
                GameLogger.LogWarning("Could not obtain current level config for level info text.", this);
                levelInfoText.text = "MISSING LEVEL CONFIG";
            }

            if (context.TryGetCustomData(LEADERBOARD_NAME_KEY, out string leaderboardName))
            {
                spectateInfoText.text = $"Spectating {leaderboardName}";
            }
            else
            {
                GameLogger.LogWarning("Could not obtain username for the spectated run.", this);
                levelInfoText.text = "MISSING USERNAME";
            }
        }
        
        private TimeRanking ProcessLevelCompletion(int milliseconds)
        {
            var currentSceneConfig = SceneLoader.Instance.CurrentSceneConfig;
            var currentLevelData = SceneLoader.Instance.CurrentLevelData;
            var levelConfig = currentSceneConfig.LevelConfig;
            var yourTime = currentLevelData.BestMilliseconds;
            var yourFormattedTime = TimerBehaviour.GetFormattedTime(yourTime);
            var yourRanking = levelConfig.GetTimeRanking(yourTime);

            oneStarText.text = TimerBehaviour.GetFormattedTime(levelConfig.OneStarMilliseconds);
            twoStarsText.text = TimerBehaviour.GetFormattedTime(levelConfig.TwoStarMilliseconds);
            threeStarsText.text = TimerBehaviour.GetFormattedTime(levelConfig.ThreeStarMilliseconds);
            rainbowText.text = TimerBehaviour.GetFormattedTime(levelConfig.RainbowMilliseconds);

            var theirRanking = levelConfig.GetTimeRanking(milliseconds);
            
            oldBestText.text = $"YOUR BEST — {yourFormattedTime}";

            if (theirRanking < TimeRanking.Rainbow && yourRanking < TimeRanking.ThreeStar)
            {
                rainbowVisibilitySetter.SetInverseState();
            }

            return theirRanking;
        }
        
        private async UniTask DisplayRankingAsync(TimeRanking ranking)
        {
            if (ranking >= TimeRanking.OneStar)
            {
                firstStar.SetActive(true);
                AudioManager.Instance.Play(AudioClipIdentifier.RankingStar);
                RumbleManager.Instance.Rumble(starRumbleConfig);
            }

            if (ranking >= TimeRanking.TwoStar)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(starAnimationDelay));
                
                if (SceneLoader.Instance.IsLoading) return;
                
                secondStar.SetActive(true);
                AudioManager.Instance.Play(AudioClipIdentifier.RankingStar);
                RumbleManager.Instance.Rumble(starRumbleConfig);
            }
            
            if (ranking >= TimeRanking.ThreeStar)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(starAnimationDelay));
                
                if (SceneLoader.Instance.IsLoading) return;
                
                thirdStar.SetActive(true);
                AudioManager.Instance.Play(AudioClipIdentifier.RankingStar);
                RumbleManager.Instance.Rumble(starRumbleConfig);
            }

            if (ranking == TimeRanking.Rainbow)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(rainbowAnimationDelay));
                
                if (SceneLoader.Instance.IsLoading) return;
                
                firstStar.SetRainbowState(true);
                secondStar.SetRainbowState(true);
                thirdStar.SetRainbowState(true);
                
                AudioManager.Instance.Play(AudioClipIdentifier.RainbowResult);
                RumbleManager.Instance.Rumble(rainbowRumbleConfig);
            }
            
            oldBestAnimator.SetTrigger(Show);
        }

        private void OnDestroy()
        {
            GhostRunner.OnSpectateVictorySequenceFinish -= HandleSpectateVictorySequenceFinish;
            
            retryButton.onClick.RemoveListener(HandleRetryClicked);
            raceButton.onClick.RemoveListener(HandleRaceClicked);
            quitButton.onClick.RemoveListener(HandleQuitClicked);
        }
    }
}