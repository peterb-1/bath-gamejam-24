using Core;
using Core.Saving;
using Cysharp.Threading.Tasks;
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
        private bool overrideNextSceneConfig;
        
        [SerializeField, ShowIf(nameof(overrideNextSceneConfig))] 
        private SceneConfig nextSceneConfig;

        private PlayerVictoryBehaviour playerVictoryBehaviour;

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

        private void HandleVictorySequenceFinish()
        {
            SetLevelInfoText();
            
            timerText.text = timerBehaviour.GetFormattedTimeElapsed();
            
            UpdateSaveData();

            victoryPageGroup.ShowGroup();
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

        private void UpdateSaveData()
        {
            var campaignData = SaveManager.Instance.SaveData.CampaignData;
            var currentSceneConfig = SceneLoader.Instance.CurrentSceneConfig;
            var shouldSave = false;
            
            if (currentSceneConfig.IsLevelScene &&
                campaignData.TryGetLevelData(currentSceneConfig.LevelConfig, out var levelData))
            {
                shouldSave |= levelData.TrySetTime(timerBehaviour.TimeElapsed);
            }

            if (currentSceneConfig.NextSceneConfig.IsLevelScene &&
                campaignData.TryGetLevelData(currentSceneConfig.NextSceneConfig.LevelConfig, out var nextLevelData))
            {
                shouldSave |= nextLevelData.TryUnlock();
            }

            if (shouldSave)
            {
                SaveManager.Instance.Save();
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