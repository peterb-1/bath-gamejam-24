using Cysharp.Threading.Tasks;
using Gameplay.Player;
using UnityEngine;

namespace Gameplay.Core
{
    public class SpawnPoint : MonoBehaviour
    {
        private Transform playerTransform;

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerTransform = PlayerAccessService.Instance.PlayerTransform;

            SpawnPlayer();
        }

        private void SpawnPlayer()
        {
            playerTransform.position = transform.position;
        }
    }
}
