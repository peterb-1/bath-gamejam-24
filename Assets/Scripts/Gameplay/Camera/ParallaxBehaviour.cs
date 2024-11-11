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

        private void Awake()
        {
            var targetCamera = UnityEngine.Camera.main;

            if (targetCamera != null)
            {
                targetCameraTransform = targetCamera.transform;
            }

            startPosition = transform.position;
        }

        private void Update()
        {
            var cameraOffset = targetCameraTransform.position.xy() - cameraStartPosition.xy();
            transform.position = startPosition - (Vector3)(followStrength * cameraOffset);
        }
    }
}
