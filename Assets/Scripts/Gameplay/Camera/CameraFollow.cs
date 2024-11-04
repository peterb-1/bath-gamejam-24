using System;
using Cysharp.Threading.Tasks;
using Gameplay.Player;
using UnityEngine;

namespace Gameplay.Camera
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] 
        private Vector3 followOffset;
        
        [SerializeField]
        private float smoothTime;

        private PlayerDeathBehaviour playerDeathBehaviour;
        private Transform target;
        private Vector3 velocity;

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);
            
            target = PlayerAccessService.Instance.PlayerTransform;
            velocity = Vector3.zero;
            
            playerDeathBehaviour = PlayerAccessService.Instance.PlayerDeathBehaviour;
            playerDeathBehaviour.OnDeathSequenceFinish += HandleDeathSequenceFinish;
        }

        private void HandleDeathSequenceFinish()
        {
            SnapToTarget();
        }

        private void Update()
        {
            transform.position = Vector3.SmoothDamp(transform.position, GetTargetPosition(), ref velocity, smoothTime);
        }

        private void SnapToTarget()
        {
            velocity = Vector3.zero;
            transform.position = GetTargetPosition();
        }

        private Vector3 GetTargetPosition()
        {
            var currentPosition = target.position;
            return new Vector3(currentPosition.x + followOffset.x, currentPosition.y + followOffset.y, followOffset.z);
        }

        private void OnDestroy()
        {
            playerDeathBehaviour.OnDeathSequenceFinish -= HandleDeathSequenceFinish;
        }
    }
}
