using System;
using Cysharp.Threading.Tasks;
using Gameplay.Colour;
using Gameplay.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Input
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] 
        private PlayerInput playerInput;
        
        [SerializeField] 
        private InputActionReference moveAction;

        [SerializeField] 
        private InputActionReference jumpAction;
        
        [SerializeField] 
        private InputActionReference blueAction;
        
        [SerializeField] 
        private InputActionReference redAction;
        
        [SerializeField] 
        private InputActionReference yellowAction;
        
        private PlayerDeathBehaviour playerDeathBehaviour;
        private PlayerVictoryBehaviour playerVictoryBehaviour;
        
        public static float MoveAmount { get; private set; }
        public static event Action<ColourId> OnColourChanged;
        public static event Action OnJumpPerformed;
        public static event Action<ControlScheme> OnControlSchemeChanged;
        
        private async void Awake()
        {
            EnableInputs();
            SubscribeToInputCallbacks();

            playerInput.onControlsChanged += HandleControlSchemeChanged;

            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerDeathBehaviour = PlayerAccessService.Instance.PlayerDeathBehaviour;
            playerDeathBehaviour.OnDeathSequenceStart += DisableInputs;
            playerDeathBehaviour.OnDeathSequenceFinish += EnableInputs;

            playerVictoryBehaviour = PlayerAccessService.Instance.PlayerVictoryBehaviour;
            playerVictoryBehaviour.OnVictorySequenceStart += HandleVictorySequenceStart;
        }

        private void HandleVictorySequenceStart(Vector2 _1, float _2)
        {
            DisableInputs();
        }

        private void HandleControlSchemeChanged(PlayerInput _)
        {
            var controlSchemeIdentifier = playerInput.currentControlScheme;
            
            var controlScheme = controlSchemeIdentifier switch
            {
                ControlSchemeIdentifiers.KEYBOARD_MOUSE => ControlScheme.KeyboardMouse,
                ControlSchemeIdentifiers.GAMEPAD => ControlScheme.Gamepad,
                _ => throw new ArgumentOutOfRangeException(nameof(controlSchemeIdentifier), controlSchemeIdentifier, null)
            };
            
            OnControlSchemeChanged?.Invoke(controlScheme);
        }

        private void EnableInputs()
        {
            moveAction.action.Enable();
            jumpAction.action.Enable();
            blueAction.action.Enable();
            redAction.action.Enable();
            yellowAction.action.Enable();
        }

        private void DisableInputs()
        {
            moveAction.action.Disable();
            jumpAction.action.Disable();
            blueAction.action.Disable();
            redAction.action.Disable();
            yellowAction.action.Disable();
        }

        private void SubscribeToInputCallbacks()
        {
            jumpAction.action.performed += HandleJumpPerformed;
            blueAction.action.performed += HandleBluePerformed;
            redAction.action.performed += HandleRedPerformed;
            yellowAction.action.performed += HandleYellowPerformed;
        }

        private void UnsubscribeFromInputCallbacks()
        {
            jumpAction.action.performed -= HandleJumpPerformed;
            blueAction.action.performed -= HandleBluePerformed;
            redAction.action.performed -= HandleRedPerformed;
            yellowAction.action.performed -= HandleYellowPerformed;
        }
        
        private void Update()
        {
            MoveAmount = moveAction.action.ReadValue<Vector2>().x;
        }

        private void HandleJumpPerformed(InputAction.CallbackContext obj)
        {
            OnJumpPerformed?.Invoke();
        }

        private static void HandleBluePerformed(InputAction.CallbackContext _)
        {
            OnColourChanged?.Invoke(ColourId.Blue);
        }
        
        private static void HandleRedPerformed(InputAction.CallbackContext _)
        {
            OnColourChanged?.Invoke(ColourId.Red);
        }
        
        private static void HandleYellowPerformed(InputAction.CallbackContext _)
        {
            OnColourChanged?.Invoke(ColourId.Yellow);
        }

        private void OnDestroy()
        {
            DisableInputs();
            UnsubscribeFromInputCallbacks();
            
            playerInput.onControlsChanged -= HandleControlSchemeChanged;
            
            playerDeathBehaviour.OnDeathSequenceStart -= DisableInputs;
            playerDeathBehaviour.OnDeathSequenceFinish -= EnableInputs;
            
            playerVictoryBehaviour.OnVictorySequenceStart -= HandleVictorySequenceStart;
        }
    }
}
