using Audio;
using Cysharp.Threading.Tasks;
using Gameplay.Camera;
using Gameplay.Input;
using Gameplay.Player;
using UnityEngine;
using Utils;

namespace UI
{
    public class TutorialPrompt : MonoBehaviour
    {
        [SerializeField] 
        private Canvas[] canvasses;
        
        [SerializeField] 
        private PageGroup displayPageGroup;
        
        [SerializeField]
        private LayerMask playerLayers;

        [SerializeField]
        private float displayVelocityThreshold;
        
        [SerializeField]
        private GameObjectStateSetter controllerStateSetter;

        private PlayerMovementBehaviour playerMovementBehaviour;
        private bool isShowing;
        
        private async void Awake()
        {
            InputManager.OnControlSchemeChanged += HandleControlSchemeChanged;
            
            await UniTask.WaitUntil(CameraAccessService.IsReady);

            foreach (var canvas in canvasses)
            {
                canvas.worldCamera = CameraAccessService.Instance.Camera;
            }
            
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerMovementBehaviour = PlayerAccessService.Instance.PlayerMovementBehaviour;
            
            HandleControlSchemeChanged(InputManager.CurrentControlScheme);
        }

        private void HandleControlSchemeChanged(ControlScheme controlScheme)
        {
            switch (controlScheme)
            {
                case ControlScheme.Gamepad:
                    controllerStateSetter.SetState();
                    break;
                case ControlScheme.Keyboard:
                    controllerStateSetter.SetInverseState();
                    break;
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (isShowing) return;
            
            if ((playerLayers.value & (1 << other.gameObject.layer)) != 0)
            {
                if (Mathf.Abs(playerMovementBehaviour.Velocity.x) < displayVelocityThreshold)
                {
                    AudioManager.Instance.Play(AudioClipIdentifier.Toggle);
                    
                    displayPageGroup.ShowGroup();
                    isShowing = true;
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!isShowing) return;
            
            if ((playerLayers.value & (1 << other.gameObject.layer)) != 0)
            {
                AudioManager.Instance.Play(AudioClipIdentifier.ButtonDenied);
                
                displayPageGroup.HideGroup();
                isShowing = false;
            }
        }

        private void OnDestroy()
        {
            InputManager.OnControlSchemeChanged -= HandleControlSchemeChanged;
        }
    }
}
