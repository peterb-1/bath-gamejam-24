using System;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Colour;
using Gameplay.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Gameplay.Input
{
    public class InputManager : MonoBehaviour
    {
        private const float PLAYER_ACCESS_TIMEOUT = 1f;
        
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
        
        public static ControlScheme CurrentControlScheme { get; private set; }
        public static float MoveAmount { get; private set; }
        public static event Action<ColourId> OnColourChanged;
        public static event Action OnJumpPerformed;
        public static event Action<ControlScheme> OnControlSchemeChanged;
        
        public static InputManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                // would usually log an error, but we expect this to happen when loading a new scene
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this);
            
            playerInput.onControlsChanged += HandleControlSchemeChanged;
            SceneLoader.OnSceneLoaded += HandleSceneLoaded;
            
            EnableInputs();
            SubscribeToInputCallbacks();
            HandleControlSchemeChanged(playerInput);
            HandleSceneLoaded();
        }
        
        private void HandleSceneLoaded()
        {
            EnableInputs();
            GetPlayerBehavioursAsync().Forget();
        }

        private async UniTask GetPlayerBehavioursAsync()
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(PLAYER_ACCESS_TIMEOUT));
            
            var isCancelled = await UniTask.WaitUntil(PlayerAccessService.IsReady, cancellationToken: cts.Token).SuppressCancellationThrow();

            if (!isCancelled)
            {
                playerDeathBehaviour = PlayerAccessService.Instance.PlayerDeathBehaviour;
                playerVictoryBehaviour = PlayerAccessService.Instance.PlayerVictoryBehaviour;

                playerDeathBehaviour.OnDeathSequenceStart += HandleDeathSequenceStart;
                playerVictoryBehaviour.OnVictorySequenceStart += HandleVictorySequenceStart;
            }
        }

        private void HandleDeathSequenceStart()
        {
            DisableInputs();
            
            playerDeathBehaviour.OnDeathSequenceStart -= HandleDeathSequenceStart;
            playerVictoryBehaviour.OnVictorySequenceStart -= HandleVictorySequenceStart;
        }

        private void HandleVictorySequenceStart(Vector2 _1, float _2)
        {
            DisableInputs();
            
            playerDeathBehaviour.OnDeathSequenceStart -= HandleDeathSequenceStart;
            playerVictoryBehaviour.OnVictorySequenceStart -= HandleVictorySequenceStart;
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
            
            if (controlScheme is ControlScheme.KeyboardMouse)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
            
            CurrentControlScheme = controlScheme;
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
            if (Instance != this) return;
            
            DisableInputs();
            UnsubscribeFromInputCallbacks();
            
            playerInput.onControlsChanged -= HandleControlSchemeChanged;
            playerDeathBehaviour.OnDeathSequenceStart -= HandleDeathSequenceStart;
            playerVictoryBehaviour.OnVictorySequenceStart -= HandleVictorySequenceStart;
            SceneLoader.OnSceneLoaded -= HandleSceneLoaded;
        }
    }
}
