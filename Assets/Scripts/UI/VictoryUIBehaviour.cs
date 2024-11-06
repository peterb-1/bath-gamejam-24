using Cysharp.Threading.Tasks;
using Gameplay.Player;
using UnityEngine;

namespace UI
{
    public class VictoryUIBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private PageGroup victoryPageGroup;

        [SerializeField] 
        private Page victoryPage;

        private PlayerVictoryBehaviour playerVictoryBehaviour;

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerVictoryBehaviour = PlayerAccessService.Instance.PlayerVictoryBehaviour;
            playerVictoryBehaviour.OnVictorySequenceFinish += HandleVictorySequenceFinish;
        }

        private void HandleVictorySequenceFinish()
        {
            victoryPageGroup.ShowGroup();
            victoryPage.Show();
        }

        private void OnDestroy()
        {
            playerVictoryBehaviour.OnVictorySequenceFinish -= HandleVictorySequenceFinish;
        }
    }
}