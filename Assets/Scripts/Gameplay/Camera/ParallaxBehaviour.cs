using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils;

namespace Gameplay.Camera
{
    public class ParallaxBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private Vector2 followStrength;

        private Transform targetCameraTransform;
        private Vector3 cameraStartPosition;
        private Vector3 startPosition;

        private async void Awake()
        {
            startPosition = transform.position;
            
            await UniTask.WaitUntil(CameraAccessService.IsReady);

            targetCameraTransform = CameraAccessService.Instance.CameraTransform;
        }

        private void Update()
        {
            var cameraOffset = targetCameraTransform.position.xy() - cameraStartPosition.xy();
            transform.position = startPosition + (Vector3)(followStrength * cameraOffset);
        }
    }
}
