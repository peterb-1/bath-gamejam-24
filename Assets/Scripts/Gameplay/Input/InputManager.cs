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
        private InputActionReference moveAction;

        [SerializeField] 
        private InputActionReference jumpAction;

        [SerializeField] 
        private InputActionReference pauseAction;
        
        [SerializeField] 
        private InputActionReference blueAction;
        
        [SerializeField] 
        private InputActionReference redAction;
        
        [SerializeField] 
        private InputActionReference yellowAction;
        
        private PlayerDeathBehaviour playerDeathBehaviour;
        private PlayerVictoryBehaviour playerVictoryBehaviour;
        
        public static ControlScheme CurrentControlScheme { get; private set; }
        public static bool AreInputsEnabled { get; private set; }
        public static float MoveAmount { get; private set; }
        public static event Action<ColourId> OnColourChanged;
        public static event Action OnJumpPerformed;
        public static event Action<ControlScheme> OnControlSchemeChanged;
        public static event Action OnPauseToggled;
        
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

        private bool IsMouseActive()
        {
            return Mouse.current != null && (Mouse.current.delta.ReadValue() != Vector2.zero || Mouse.current.leftButton.wasPressedThisFrame);
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
            MoveAmount = moveAction.action.ReadValue<Vector2>().x;
            
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
                
                EventSystem.current.SetSelectedGameObject(null);
            }
            else
            {
                Cursor.visible = false;
            }

            OnControlSchemeChanged?.Invoke(controlScheme);
        }

        private void EnableInputs()
        {
            AreInputsEnabled = true;
            
            moveAction.action.Enable();
            jumpAction.action.Enable();
            pauseAction.action.Enable();
            blueAction.action.Enable();
            redAction.action.Enable();
            yellowAction.action.Enable();
        }

        private void DisableInputs()
        {
            AreInputsEnabled = false;
            
            moveAction.action.Disable();
            jumpAction.action.Disable();
            pauseAction.action.Disable();
            blueAction.action.Disable();
            redAction.action.Disable();
            yellowAction.action.Disable();
        }

        private void SubscribeToInputCallbacks()
        {
            jumpAction.action.performed += HandleJumpPerformed;
            pauseAction.action.performed += HandlePausePerformed;
            blueAction.action.performed += HandleBluePerformed;
            redAction.action.performed += HandleRedPerformed;
            yellowAction.action.performed += HandleYellowPerformed;
        }

        private void UnsubscribeFromInputCallbacks()
        {
            jumpAction.action.performed -= HandleJumpPerformed;
            pauseAction.action.performed -= HandlePausePerformed;
            blueAction.action.performed -= HandleBluePerformed;
            redAction.action.performed -= HandleRedPerformed;
            yellowAction.action.performed -= HandleYellowPerformed;
        }

        private static void HandleJumpPerformed(InputAction.CallbackContext _)
        {
            if (PauseManager.Instance == null || PauseManager.Instance.IsPaused) return;
            OnJumpPerformed?.Invoke();
        }

        private static void HandlePausePerformed(InputAction.CallbackContext _)
        {
            OnPauseToggled?.Invoke();
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
        }
    }
}
