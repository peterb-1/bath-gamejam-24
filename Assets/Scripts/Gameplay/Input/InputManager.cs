using System;
using Gameplay.Colour;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Input
{
    public class InputManager : MonoBehaviour
    {
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
        
        public static float MoveAmount { get; private set; }
        public static event Action<ColourId> OnColourChanged;
        public static event Action OnJumpPerformed;
        
        private void Awake()
        {
            SetupInputs();
        }

        private void SetupInputs()
        {
            moveAction.action.Enable();
            jumpAction.action.Enable();
            blueAction.action.Enable();
            redAction.action.Enable();
            yellowAction.action.Enable();

            jumpAction.action.performed += HandleJumpPerformed;
            blueAction.action.performed += HandleBluePerformed;
            redAction.action.performed += HandleRedPerformed;
            yellowAction.action.performed += HandleYellowPerformed;
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
            ShutdownInputs();
        }
        
        private void ShutdownInputs()
        {
            moveAction.action.Disable();
            jumpAction.action.Disable();
            blueAction.action.Disable();
            redAction.action.Disable();
            yellowAction.action.Disable();

            jumpAction.action.performed -= HandleJumpPerformed;
            blueAction.action.performed -= HandleBluePerformed;
            redAction.action.performed -= HandleRedPerformed;
            yellowAction.action.performed -= HandleYellowPerformed;
        }
    }
}
