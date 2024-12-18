using System;
using Core;
using Core.Saving;
using Cysharp.Threading.Tasks;
using Gameplay.Core;
using Gameplay.Player;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class VictoryUIBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private PageGroup victoryPageGroup;

        [SerializeField] 
        private Button retryButton;
        
        [SerializeField] 
        private Button nextButton;

        [SerializeField] 
        private Button quitButton;
        
        [SerializeField] 
        private TimerBehaviour timerBehaviour;

        [SerializeField] 
        private TMP_Text timerText;
        
        [SerializeField] 
        private TMP_Text levelInfoText;

        [SerializeField] 
        private TMP_Text oneStarText;
        
        [SerializeField] 
        private TMP_Text twoStarsText;
        
        [SerializeField] 
        private TMP_Text threeStarsText;

        [SerializeField] 
        private TMP_Text rainbowText;
        
        [SerializeField] 
        private Animator newBestAnimator;

        [SerializeField] 
        private RankingStar firstStar;
        
        [SerializeField] 
        private RankingStar secondStar;
        
        [SerializeField] 
        private RankingStar thirdStar;

        [SerializeField] 
        private float starAnimationDelay;

        [SerializeField] 
        private bool overrideNextSceneConfig;
        
        [SerializeField, ShowIf(nameof(overrideNextSceneConfig))] 
        private SceneConfig nextSceneConfig;

        private PlayerVictoryBehaviour playerVictoryBehaviour;
        private static readonly int Show = Animator.StringToHash("Show");

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerVictoryBehaviour = PlayerAccessService.Instance.PlayerVictoryBehaviour;
            playerVictoryBehaviour.OnVictorySequenceFinish += HandleVictorySequenceFinish;
            
            retryButton.onClick.AddListener(HandleRetryClicked);
            nextButton.onClick.AddListener(HandleNextClicked);
            quitButton.onClick.AddListener(HandleQuitClicked);
        }

        private void HandleRetryClicked()
        {
            victoryPageGroup.HideGroup();
            SceneLoader.Instance.ReloadCurrentScene();
        }
        
        private void HandleNextClicked()
        {
            victoryPageGroup.HideGroup();

            var sceneToLoad = overrideNextSceneConfig 
                ? nextSceneConfig 
                : SceneLoader.Instance.CurrentSceneConfig.NextSceneConfig;
            
            SceneLoader.Instance.LoadScene(sceneToLoad);
        }
        
        private void HandleQuitClicked()
        {
            victoryPageGroup.HideGroup();
            SceneLoader.Instance.LoadLevelSelect();
        }

        private async void HandleVictorySequenceFinish()
        {
            SetLevelInfoText();
            
            timerText.text = timerBehaviour.GetFormattedTimeElapsed();
            
            var (ranking, isNewBest) = ProcessLevelCompletion();

            await victoryPageGroup.ShowGroupAsync();
            await DisplayRankingAsync(ranking, isNewBest);
        }
        
        private void SetLevelInfoText()
        {
            var sceneConfig = SceneLoader.Instance.CurrentSceneConfig;

            if (sceneConfig.IsLevelScene)
            {
                levelInfoText.text = sceneConfig.LevelConfig.GetLevelText();
            }
            else
            {
                GameLogger.LogWarning("Could not obtain current level config for level info text.", this);
                levelInfoText.text = "MISSING LEVEL CONFIG";
            }
        }

        private (TimeRanking, bool) ProcessLevelCompletion()
        {
            var campaignData = SaveManager.Instance.SaveData.CampaignData;
            var currentSceneConfig = SceneLoader.Instance.CurrentSceneConfig;
            var shouldSave = false;

            var ranking = TimeRanking.Unranked;
            var isNewBest = false;
            
            if (currentSceneConfig.IsLevelScene &&
                campaignData.TryGetLevelData(currentSceneConfig.LevelConfig, out var levelData))
            {
                var levelConfig = currentSceneConfig.LevelConfig;
                var time = timerBehaviour.TimeElapsed;

                oneStarText.text = TimerBehaviour.GetFormattedTime(levelConfig.OneStarTime);
                twoStarsText.text = TimerBehaviour.GetFormattedTime(levelConfig.TwoStarTime);
                threeStarsText.text = TimerBehaviour.GetFormattedTime(levelConfig.ThreeStarTime);
                rainbowText.text = TimerBehaviour.GetFormattedTime(levelConfig.RainbowTime);

                ranking = levelConfig.GetTimeRanking(time);
                isNewBest = levelData.TrySetTime(time);
                shouldSave |= isNewBest;

                if (!isNewBest)
                {
                    newBestAnimator.gameObject.SetActive(false);
                }
            }

            if (currentSceneConfig.NextSceneConfig != null && 
                currentSceneConfig.NextSceneConfig.IsLevelScene &&
                campaignData.TryGetLevelData(currentSceneConfig.NextSceneConfig.LevelConfig, out var nextLevelData))
            {
                shouldSave |= nextLevelData.TryUnlock();
            }

            if (shouldSave)
            {
                SaveManager.Instance.Save();
            }

            return (ranking, isNewBest);
        }

        private async UniTask DisplayRankingAsync(TimeRanking ranking, bool isNewBest)
        {
            if (ranking >= TimeRanking.OneStar)
            {
                firstStar.SetActive(true);
            }

            if (ranking >= TimeRanking.TwoStar)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(starAnimationDelay));
                secondStar.SetActive(true);
            }
            
            if (ranking >= TimeRanking.ThreeStar)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(starAnimationDelay));
                thirdStar.SetActive(true);
            }

            if (ranking == TimeRanking.Rainbow)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(starAnimationDelay));
                
                firstStar.SetRainbowState(true);
                secondStar.SetRainbowState(true);
                thirdStar.SetRainbowState(true);
            }

            if (isNewBest)
            {
                newBestAnimator.SetTrigger(Show);
            }
        }

        private void OnDestroy()
        {
            playerVictoryBehaviour.OnVictorySequenceFinish -= HandleVictorySequenceFinish;
            
            retryButton.onClick.RemoveListener(HandleRetryClicked);
            nextButton.onClick.RemoveListener(HandleNextClicked);
            quitButton.onClick.RemoveListener(HandleQuitClicked);
        }
    }
}