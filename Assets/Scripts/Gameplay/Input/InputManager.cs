using System;
using Gameplay.Colour;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Input
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] 
        private InputActionReference blueAction;
        
        [SerializeField] 
        private InputActionReference redAction;
        
        [SerializeField] 
        private InputActionReference yellowAction;
        
        public static event Action<ColourId> OnColourChanged;
        
        private void Awake()
        {
            SetupInputs();
        }

        private void SetupInputs()
        {
            blueAction.action.Enable();
            redAction.action.Enable();
            yellowAction.action.Enable();

            blueAction.action.performed += HandleBluePerformed;
            redAction.action.performed += HandleRedPerformed;
            yellowAction.action.performed += HandleYellowPerformed;
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
            blueAction.action.Disable();
            redAction.action.Disable();
            yellowAction.action.Disable();

            blueAction.action.performed -= HandleBluePerformed;
            redAction.action.performed -= HandleRedPerformed;
            yellowAction.action.performed -= HandleYellowPerformed;
        }
    }
}
