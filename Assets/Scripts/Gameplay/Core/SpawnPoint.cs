using System;
using Cysharp.Threading.Tasks;
using Gameplay.Player;
using UnityEngine;

namespace Gameplay.Core
{
    public class SpawnPoint : MonoBehaviour
    {
        private PlayerDeathBehaviour playerDeathBehaviour;
        private Transform playerTransform;

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerDeathBehaviour = PlayerAccessService.Instance.PlayerDeathBehaviour;
            playerTransform = PlayerAccessService.Instance.PlayerTransform;

            SpawnPlayer();

            playerDeathBehaviour.OnDeathSequenceFinish += HandleDeathSequenceFinish;
        }

        private void HandleDeathSequenceFinish()
        {
            SpawnPlayer();
        }

        private void SpawnPlayer()
        {
            playerTransform.position = transform.position;
        }

        private void OnDestroy()
        {
            playerDeathBehaviour.OnDeathSequenceFinish -= HandleDeathSequenceFinish;
        }
    }
}
