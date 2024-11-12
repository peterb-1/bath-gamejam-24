using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Player;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        private TMP_Text timerText;

        [SerializeField] 
        private int nextSceneIndex;

        private float time;

        private PlayerVictoryBehaviour playerVictoryBehaviour;

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerVictoryBehaviour = PlayerAccessService.Instance.PlayerVictoryBehaviour;
            playerVictoryBehaviour.OnVictorySequenceFinish += HandleVictorySequenceFinish;
            
            retryButton.onClick.AddListener(HandleRetryClicked);
            nextButton.onClick.AddListener(HandleNextClicked);
        }

        private void Start()
        {
            time = Time.time;
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

        private void FormatTime()
        {
            var seconds = (int)(time % 60);
            var centiSeconds = (int)((time - (int)time)*100);
            var minutes = (int)(time / 60);
            timerText.text = $"{minutes:00}:{seconds:00}:{centiSeconds:00}";
        }
        
        private void HandleVictorySequenceFinish()
        {
            victoryPageGroup.ShowGroup();

            time = Time.time - time;
            FormatTime();
        }

        private void OnDestroy()
        {
            playerVictoryBehaviour.OnVictorySequenceFinish -= HandleVictorySequenceFinish;
            
            retryButton.onClick.RemoveListener(HandleRetryClicked);
            nextButton.onClick.RemoveListener(HandleNextClicked);
        }
    }
}