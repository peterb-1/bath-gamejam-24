using System;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Colour;
using Gameplay.Core;
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
        private InputActionReference startAction;
        
        [SerializeField] 
        private InputActionReference backAction;

        [SerializeField] 
        private InputActionReference moveAction;

        [SerializeField] 
        private InputActionReference jumpAction;

        [SerializeField] 
        private InputActionReference dropAction;
        
        [SerializeField] 
        private InputActionReference dashAction;

        [SerializeField] 
        private InputActionReference pauseAction;
        
        [SerializeField] 
        private InputActionReference restartAction;
        
        [SerializeField] 
        private InputActionReference blueAction;
        
        [SerializeField] 
        private InputActionReference redAction;
        
        [SerializeField] 
        private InputActionReference yellowAction;
        
        [SerializeField] 
        private float gamepadDropThreshold;

        [SerializeField] 
        private float movementThreshold;
        
        [SerializeField, Tooltip("In pixels moved per frame")]
        private int mouseDetectionThreshold;
        
        private PlayerDeathBehaviour playerDeathBehaviour;
        private PlayerVictoryBehaviour playerVictoryBehaviour;

        private Vector2 unhiddenMousePosition;
        private static bool isPrimedForGamepadDrop;
        
        public static ControlScheme CurrentControlScheme { get; private set; }
        public static ControlScheme CurrentNonMouseControlScheme { get; private set; }
        public static bool AreInputsEnabled { get; private set; }
        public static float MoveAmount { get; private set; }
        public static event Action<ColourId> OnColourChanged;
        public static event Action OnStartPerformed;
        public static event Action OnBackPerformed;
        public static event Action OnJumpPerformed;
        public static event Action OnDropPerformed;
        public static event Action OnDashPerformed;
        public static event Action OnPauseToggled;
        public static event Action OnRestartPerformed;
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
            transform.parent = null;
            DontDestroyOnLoad(this);
            
            playerInput.onControlsChanged += HandleControlSchemeChanged;
            SceneLoader.OnSceneLoaded += HandleSceneLoaded;
            SceneLoader.OnSceneLoadStart += HandleSceneLoadStart;
            
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
        
        private void HandleSceneLoadStart()
        {
            DisableInputs();
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

        private void HandleDeathSequenceStart(PlayerDeathSource _)
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

        private bool IsMouseActive()
        {
            if (Mouse.current == null) return false;

            var mouseValue = Mouse.current.delta.ReadValue();
            var isMouseMoving = Mathf.Abs(mouseValue.x) >= mouseDetectionThreshold || Mathf.Abs(mouseValue.y) >= mouseDetectionThreshold;
            
            return isMouseMoving || Mouse.current.leftButton.wasPressedThisFrame;
        }

        private async void HandleControlSchemeChanged(PlayerInput _)
        {
            // stupid hack to ensure that the mouse is recognised as active if that's what triggered the callback
            await UniTask.Yield();
            
            var controlSchemeIdentifier = playerInput.currentControlScheme;
            
            var controlScheme = (controlSchemeIdentifier, IsMouseActive()) switch
            {
                (ControlSchemeIdentifiers.KEYBOARD_MOUSE, true) => ControlScheme.Mouse,
                (ControlSchemeIdentifiers.KEYBOARD_MOUSE, false) => ControlScheme.Keyboard,
                (ControlSchemeIdentifiers.GAMEPAD, _) => ControlScheme.Gamepad,
                _ => throw new ArgumentOutOfRangeException(nameof(controlSchemeIdentifier), controlSchemeIdentifier, null)
            };
            
            UpdateControlScheme(controlScheme);
        }
        
        private void Update()
        {
            var moveVector = moveAction.action.ReadValue<Vector2>();
            var horizontalAmount = moveVector.x;
            var verticalAmount = moveVector.y;

            MoveAmount = Mathf.Abs(horizontalAmount) > movementThreshold
                ? Mathf.Sign(horizontalAmount)
                : 0f;

            isPrimedForGamepadDrop = verticalAmount < gamepadDropThreshold && CurrentControlScheme is ControlScheme.Gamepad;
            
            if (CurrentControlScheme is not ControlScheme.Mouse && IsMouseActive())
            {
                UpdateControlScheme(ControlScheme.Mouse);
            }

            if (CurrentControlScheme is ControlScheme.Mouse)
            {
                if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                {
                    UpdateControlScheme(ControlScheme.Keyboard);
                }
            }
        }

        private void UpdateControlScheme(ControlScheme controlScheme)
        {
            CurrentControlScheme = controlScheme;

            if (controlScheme is ControlScheme.Mouse)
            {
                Cursor.visible = true;
                
                Mouse.current.WarpCursorPosition(unhiddenMousePosition);

                EventSystem.current.SetSelectedGameObject(null);
            }
            else
            {
                Cursor.visible = false;

                unhiddenMousePosition = Mouse.current.position.ReadValue();
                
                Mouse.current.WarpCursorPosition(new Vector2(0, 0));

                CurrentNonMouseControlScheme = controlScheme;
            }

            OnControlSchemeChanged?.Invoke(controlScheme);
        }

        private void EnableInputs()
        {
            AreInputsEnabled = true;
            
            startAction.action.Enable();
            backAction.action.Enable();
            moveAction.action.Enable();
            jumpAction.action.Enable();
            dropAction.action.Enable();
            dashAction.action.Enable();
            pauseAction.action.Enable();
            restartAction.action.Enable();
            blueAction.action.Enable();
            redAction.action.Enable();
            yellowAction.action.Enable();
        }

        private void DisableInputs()
        {
            AreInputsEnabled = false;
            
            startAction.action.Disable();
            backAction.action.Disable();
            moveAction.action.Disable();
            jumpAction.action.Disable();
            dropAction.action.Disable();
            dashAction.action.Disable();
            pauseAction.action.Disable();
            restartAction.action.Disable();
            blueAction.action.Disable();
            redAction.action.Disable();
            yellowAction.action.Disable();
        }

        private void SubscribeToInputCallbacks()
        {
            startAction.action.performed += HandleStartPerformed;
            backAction.action.performed += HandleBackPerformed;
            jumpAction.action.performed += HandleJumpPerformed;
            dropAction.action.performed += HandleDropPerformed;
            dashAction.action.performed += HandleDashPerformed;
            pauseAction.action.performed += HandlePausePerformed;
            restartAction.action.performed += HandleRestartPerformed;
            blueAction.action.performed += HandleBluePerformed;
            redAction.action.performed += HandleRedPerformed;
            yellowAction.action.performed += HandleYellowPerformed;
        }

        private void UnsubscribeFromInputCallbacks()
        {
            startAction.action.performed -= HandleStartPerformed;
            backAction.action.performed -= HandleBackPerformed;
            jumpAction.action.performed -= HandleJumpPerformed;
            dropAction.action.performed -= HandleDropPerformed;
            dashAction.action.performed -= HandleDashPerformed;
            pauseAction.action.performed -= HandlePausePerformed;
            restartAction.action.performed -= HandleRestartPerformed;
            blueAction.action.performed -= HandleBluePerformed;
            redAction.action.performed -= HandleRedPerformed;
            yellowAction.action.performed -= HandleYellowPerformed;
        }
        
        private void HandleStartPerformed(InputAction.CallbackContext _)
        {
            OnStartPerformed?.Invoke();
        }
        
        private void HandleBackPerformed(InputAction.CallbackContext _)
        {
            OnBackPerformed?.Invoke();
        }

        private static void HandleJumpPerformed(InputAction.CallbackContext _)
        {
            if (PauseManager.Instance == null || PauseManager.Instance.IsPaused) return;
            
            if (isPrimedForGamepadDrop)
            {
                OnDropPerformed?.Invoke();
            }

            OnJumpPerformed?.Invoke();
        }
        
        private static void HandleDropPerformed(InputAction.CallbackContext _)
        {
            if (PauseManager.Instance == null || PauseManager.Instance.IsPaused) return;
            OnDropPerformed?.Invoke();
        }
        
        private static void HandleDashPerformed(InputAction.CallbackContext _)
        {
            if (PauseManager.Instance == null || PauseManager.Instance.IsPaused) return;
            OnDashPerformed?.Invoke();
        }

        private static void HandlePausePerformed(InputAction.CallbackContext _)
        {
            OnPauseToggled?.Invoke();
        }
        
        private static void HandleRestartPerformed(InputAction.CallbackContext _)
        {
            if (PauseManager.Instance == null || PauseManager.Instance.IsPaused) return;
            OnRestartPerformed?.Invoke();
        }

        private static void HandleBluePerformed(InputAction.CallbackContext _)
        {
            if (PauseManager.Instance == null || PauseManager.Instance.IsPaused) return;
            OnColourChanged?.Invoke(ColourId.Blue);
        }
        
        private static void HandleRedPerformed(InputAction.CallbackContext _)
        {
            if (PauseManager.Instance == null || PauseManager.Instance.IsPaused) return;
            OnColourChanged?.Invoke(ColourId.Red);
        }
        
        private static void HandleYellowPerformed(InputAction.CallbackContext _)
        {
            if (PauseManager.Instance == null || PauseManager.Instance.IsPaused) return;
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
            SceneLoader.OnSceneLoadStart -= HandleSceneLoadStart;
        }
    }
}
