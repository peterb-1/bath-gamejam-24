using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        private TimerBehaviour timerBehaviour;

        [SerializeField] 
        private TMP_Text timerText;

        [SerializeField] 
        private int nextSceneIndex;

        private PlayerVictoryBehaviour playerVictoryBehaviour;

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerVictoryBehaviour = PlayerAccessService.Instance.PlayerVictoryBehaviour;
            playerVictoryBehaviour.OnVictorySequenceFinish += HandleVictorySequenceFinish;
            
            retryButton.onClick.AddListener(HandleRetryClicked);
            nextButton.onClick.AddListener(HandleNextClicked);
        }

        private void HandleRetryClicked()
        {
            victoryPageGroup.HideGroup();
            
            SceneLoader.Instance.ReloadCurrentScene();
        }
        
        private void HandleNextClicked()
        {
            victoryPageGroup.HideGroup();
            
            SceneLoader.Instance.LoadScene(nextSceneIndex);
        }

        private void HandleVictorySequenceFinish()
        {
            victoryPageGroup.ShowGroup();

            timerText.text = timerBehaviour.GetFormattedTimeElapsed();
        }

        private void OnDestroy()
        {
            playerVictoryBehaviour.OnVictorySequenceFinish -= HandleVictorySequenceFinish;
            
            retryButton.onClick.RemoveListener(HandleRetryClicked);
            nextButton.onClick.RemoveListener(HandleNextClicked);
        }
    }
}