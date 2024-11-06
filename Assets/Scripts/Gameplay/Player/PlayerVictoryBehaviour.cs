using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils;

namespace Gameplay.Player
{
    public class PlayerVictoryBehaviour : MonoBehaviour
    {
        [SerializeField]
        private float victorySequenceDuration;
        
        [SerializeField] 
        private LayerMask victoryLayers;

        [SerializeField] 
        private BoxCollider2D playerHitbox;
        
        public event Action<Vector2, float> OnVictorySequenceStart;
        public event Action OnVictorySequenceFinish;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((victoryLayers.value & (1 << other.gameObject.layer)) != 0)
            {
                RunVictorySequenceAsync(other.transform.position).Forget();
            }
        }

        private async UniTask RunVictorySequenceAsync(Vector3 targetPosition)
        {
            OnVictorySequenceStart?.Invoke(targetPosition, victorySequenceDuration);

            playerHitbox.enabled = false;

            await UniTask.Delay(TimeSpan.FromSeconds(victorySequenceDuration));

            OnVictorySequenceFinish?.Invoke();
        }
    }
}