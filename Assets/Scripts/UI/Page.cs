using Cysharp.Threading.Tasks;
using Gameplay.Input;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class Page : MonoBehaviour
    {
        [SerializeField] 
        private Animator pageAnimator;

        [SerializeField] 
        private bool hasDefaultSelectable;

        [SerializeField, ShowIf(nameof(hasDefaultSelectable))]
        private Selectable defaultSelectable;

        private static readonly int IsActive = Animator.StringToHash("isActive");
        private static readonly int IsForward = Animator.StringToHash("isActive");

        private bool isActive;

        // needs to be Start, not Awake, so that the default selectable is definitely awake when trying to select it
        private void Start()
        {
            InputManager.OnControlSchemeChanged += HandleControlSchemeChanged;
            
            HandleControlSchemeChanged(InputManager.CurrentControlScheme);
        }
        
        private void HandleControlSchemeChanged(ControlScheme controlScheme)
        {
            if (isActive && hasDefaultSelectable && controlScheme is not ControlScheme.Mouse)
            {
                EventSystem.current.SetSelectedGameObject(null);
                defaultSelectable.Select();
            }
        }

        public void Show(bool isForward = true)
        {
            isActive = true;
                
            pageAnimator.SetBool(IsForward, isForward);
            pageAnimator.SetBool(IsActive, true);

            if (hasDefaultSelectable && InputManager.CurrentControlScheme is not ControlScheme.Mouse)
            {
                EventSystem.current.SetSelectedGameObject(null);
                defaultSelectable.Select();
            }
        }

        public void Hide(bool isForward = true)
        {
            if (EventSystem.current.currentSelectedGameObject != null &&
                EventSystem.current.currentSelectedGameObject.transform.IsChildOf(transform))
            {
                EventSystem.current.SetSelectedGameObject(null);
            }

            isActive = false;
            
            pageAnimator.SetBool(IsForward, isForward);
            pageAnimator.SetBool(IsActive, false);
        }

        public void ShowImmediate()
        {
            isActive = true;
            
            pageAnimator.SetBool(IsActive, true);
            pageAnimator.EvaluateCurrentClipEndAsync().Forget();
            
            if (hasDefaultSelectable && InputManager.CurrentControlScheme is not ControlScheme.Mouse)
            {
                EventSystem.current.SetSelectedGameObject(null);
                defaultSelectable.Select();
            }
        }
        
        public void HideImmediate()
        {
            if (EventSystem.current.currentSelectedGameObject != null &&
                EventSystem.current.currentSelectedGameObject.transform.IsChildOf(transform))
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
            
            isActive = false;
            
            pageAnimator.SetBool(IsActive, false);
            pageAnimator.EvaluateCurrentClipEndAsync().Forget();
        }
        
        private void OnDestroy()
        {
            InputManager.OnControlSchemeChanged -= HandleControlSchemeChanged;
        }
    }
}
