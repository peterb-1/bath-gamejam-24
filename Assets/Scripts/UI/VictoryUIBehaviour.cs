using System;
using Audio;
using Core;
using Core.Saving;
using Cysharp.Threading.Tasks;
using Gameplay.Core;
using Gameplay.Ghosts;
using Gameplay.Player;
using Hardware;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class VictoryUIBehaviour : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] 
        private PageGroup victoryPageGroup;
        
        [SerializeField] 
        private Page victoryPage;

        [SerializeField] 
        private Button retryButton;
        
        [SerializeField] 
        private Button nextButton;

        [SerializeField] 
        private Button quitButton;

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
        private TMP_Text oldBestText;
        
        [SerializeField] 
        private TMP_Text collectibleStatusText;

        [SerializeField] 
        private Animator oldBestAnimator;
        
        [SerializeField] 
        private Animator newBestAnimator;

        [SerializeField] 
        private RankingStar firstStar;
        
        [SerializeField] 
        private RankingStar secondStar;
        
        [SerializeField] 
        private RankingStar thirdStar;

        [SerializeField] 
        private GameObject[] collectibleGameObjects;

        [SerializeField]
        private CollectibleUIBehaviour collectibleUIBehaviour;

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
        private float newRecordDelay;

        [SerializeField] 
        private GameObjectStateSetter rainbowVisibilitySetter;

        [SerializeField] 
        private bool overrideNextSceneConfig;
        
        [SerializeField, ShowIf(nameof(overrideNextSceneConfig))] 
        private SceneConfig nextSceneConfig;

        private PlayerVictoryBehaviour playerVictoryBehaviour;
        private GhostWriter ghostWriter;
        
        private static readonly int Show = Animator.StringToHash("Show");

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerVictoryBehaviour = PlayerAccessService.Instance.PlayerVictoryBehaviour;
            playerVictoryBehaviour.OnVictorySequenceFinish += HandleVictorySequenceFinish;

            ghostWriter = PlayerAccessService.Instance.GhostWriter;
            
            retryButton.onClick.AddListener(HandleRetryClicked);
            nextButton.onClick.AddListener(HandleNextClicked);
            quitButton.onClick.AddListener(HandleQuitClicked);

            if (SceneLoader.Instance.CurrentSceneConfig.LevelConfig.LevelType is LevelType.Hidden)
            {
                nextButton.gameObject.SetActive(false);
                victoryPage.SetDefaultSelectable(retryButton);

                var retryNavigation = retryButton.navigation;
                var quitNavigation = quitButton.navigation;

                retryNavigation.selectOnRight = quitButton;
                quitNavigation.selectOnLeft = retryButton;

                retryButton.navigation = retryNavigation;
                quitButton.navigation = quitNavigation;
            }
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
                : SceneLoader.Instance.CurrentSceneConfig.UnlockedConfigsOnCompletion[0];
            
            SceneLoader.Instance.LoadScene(sceneToLoad);
        }
        
        private void HandleQuitClicked()
        {
            victoryPageGroup.HideGroup();
            SceneLoader.Instance.LoadLevelSelect();
        }

        private async void HandleVictorySequenceFinish(int finalMilliseconds, bool hasCollectible)
        {
            SetLevelInfoText();
            
            timerText.text = TimerBehaviour.GetFormattedTime(finalMilliseconds);
            
            var (ranking, isNewBest) = ProcessLevelCompletion(finalMilliseconds, hasCollectible);

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

        private (TimeRanking, bool) ProcessLevelCompletion(int milliseconds, bool hasFoundCollectible)
        {
            var campaignData = SaveManager.Instance.SaveData.CampaignData;
            var currentSceneConfig = SceneLoader.Instance.CurrentSceneConfig;
            var currentLevelData = SceneLoader.Instance.CurrentLevelData;
            var levelConfig = currentSceneConfig.LevelConfig;
            var oldMilliseconds = currentLevelData.BestMilliseconds;
            var oldFormattedTime = TimerBehaviour.GetFormattedTime(oldMilliseconds);
            var oldRanking = levelConfig.GetTimeRanking(oldMilliseconds);
            var doFormattedTimesMatch = oldFormattedTime == TimerBehaviour.GetFormattedTime(milliseconds);

            oneStarText.text = TimerBehaviour.GetFormattedTime(levelConfig.OneStarMilliseconds);
            twoStarsText.text = TimerBehaviour.GetFormattedTime(levelConfig.TwoStarMilliseconds);
            threeStarsText.text = TimerBehaviour.GetFormattedTime(levelConfig.ThreeStarMilliseconds);
            rainbowText.text = TimerBehaviour.GetFormattedTime(levelConfig.RainbowMilliseconds);

            var ranking = levelConfig.GetTimeRanking(milliseconds);
            var isNewBest = currentLevelData.TrySetTime(milliseconds) && !doFormattedTimesMatch;
            var shouldSave = isNewBest;

            if (isNewBest)
            {
                oldBestAnimator.gameObject.SetActive(false);
                ghostWriter.SaveGhostData();
            }
            else
            {
                oldBestText.text = $"BEST — {oldFormattedTime}";
                newBestAnimator.gameObject.SetActive(false);
            }
            
            var bestRanking = ranking > oldRanking ? ranking : oldRanking;
            if (bestRanking < TimeRanking.ThreeStar)
            {
                rainbowVisibilitySetter.SetInverseState();
            }

            if (levelConfig.HasCollectible)
            {
                if (hasFoundCollectible)
                {
                    shouldSave |= campaignData.TryMarkCollectibleAsFound(levelConfig);
                }

                collectibleStatusText.text = currentLevelData.HasFoundCollectible ? "Comms panel retrieved!" : "Not yet found";
                collectibleUIBehaviour.SetCollected(currentLevelData.HasFoundCollectible);
            }
            else
            {
                foreach (var obj in collectibleGameObjects)
                {
                    obj.SetActive(false);
                }
            }
            
            GameLogger.Log($"{currentSceneConfig.name} was completed in {milliseconds.ToSeconds()}s - awarding ranking of {ranking}!", this);

            foreach (var sceneConfig in currentSceneConfig.UnlockedConfigsOnCompletion)
            {
                if (sceneConfig.IsLevelScene && campaignData.TryGetLevelData(sceneConfig.LevelConfig, out var nextLevelData))
                {
                    shouldSave |= nextLevelData.TryUnlock();
                }
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

            if (isNewBest)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(newRecordDelay));
                
                if (SceneLoader.Instance.IsLoading) return;
                
                AudioManager.Instance.Play(AudioClipIdentifier.NewRecord);
                newBestAnimator.SetTrigger(Show);
            }
            else
            {
                oldBestAnimator.SetTrigger(Show);
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