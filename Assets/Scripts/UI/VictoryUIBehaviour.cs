using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Player;
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

        private PlayerVictoryBehaviour playerVictoryBehaviour;

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerVictoryBehaviour = PlayerAccessService.Instance.PlayerVictoryBehaviour;
            playerVictoryBehaviour.OnVictorySequenceFinish += HandleVictorySequenceFinish;
            
            retryButton.onClick.AddListener(HandleRetryClicked);
        }

        private void HandleRetryClicked()
        {
            victoryPageGroup.HideGroup();
            
            SceneLoader.Instance.ReloadCurrentScene();
        }

        private void HandleVictorySequenceFinish()
        {
            victoryPageGroup.ShowGroup();
        }

        private void OnDestroy()
        {
            playerVictoryBehaviour.OnVictorySequenceFinish -= HandleVictorySequenceFinish;
            
            retryButton.onClick.RemoveListener(HandleRetryClicked);
        }
    }
}