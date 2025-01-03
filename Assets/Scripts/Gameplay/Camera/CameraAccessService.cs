using UnityEngine;
using Utils;

namespace Gameplay.Camera
{
    public class CameraAccessService : MonoBehaviour
    {
        [field: SerializeField]
        public UnityEngine.Camera Camera { get; private set; }
        
        [field: SerializeField]
        public Transform CameraTransform { get; private set; }
        
        [field: SerializeField]
        public CameraFollow CameraFollow { get; private set; }
        
        public static CameraAccessService Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                GameLogger.LogError("Cannot have more than one CameraAccessService in the scene at once! Destroying this one.");
                Destroy(this);
                return;
            }

            Instance = this;
        }

        public static bool IsReady() => Instance != null;

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}