using System;
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
        private CanvasGroup canvasGroup;

        [SerializeField] 
        private bool hasDefaultSelectable;

        [SerializeField, ShowIf(nameof(hasDefaultSelectable))]
        private Selectable defaultSelectable;

        [SerializeField, ShowIf(nameof(hasDefaultSelectable))]
        private bool selectOnShow = true;

        private static readonly int IsActive = Animator.StringToHash("isActive");
        private static readonly int IsForward = Animator.StringToHash("isForward");

        private bool isActive;
        
        public event Action OnShown;
        public event Action OnHidden;

        // needs to be Start, not Awake, so that the default selectable is definitely awake when trying to select it
        private void Start()
        {
            InputManager.OnControlSchemeChanged += HandleControlSchemeChanged;
            
            if (selectOnShow && isActive)
            {
                TrySelectDefaultSelectable();
            }
        }
        
        private void HandleControlSchemeChanged(ControlScheme controlScheme)
        {
            if (isActive)
            {
                TrySelectDefaultSelectable();
            }
        }
        
        public async UniTask ShowAsync(bool isForward = true)
        {
            isActive = true;
                
            pageAnimator.SetBool(IsForward, isForward);
            pageAnimator.SetBool(IsActive, true);
            
            var duration = await pageAnimator.GetCurrentClipDurationAsync();
            await UniTask.Delay(TimeSpan.FromSeconds(duration));

            // has been destroyed or hidden in the meantime
            if (canvasGroup == null || !isActive) return;
            
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            if (selectOnShow)
            {
                TrySelectDefaultSelectable();
            }
            
            OnShown?.Invoke();
        }
        
        public async UniTask HideAsync(bool isForward = true)
        {
            OnHidden?.Invoke();
            
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            if (EventSystem.current.currentSelectedGameObject != null &&
                EventSystem.current.currentSelectedGameObject.transform.IsChildOf(transform))
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
            
            isActive = false;

            pageAnimator.SetBool(IsForward, isForward);
            pageAnimator.SetBool(IsActive, false);

            var duration = await pageAnimator.GetCurrentClipDurationAsync();
            await UniTask.Delay(TimeSpan.FromSeconds(duration));
        }

        public void Show(bool isForward = true)
        {
            ShowAsync(isForward).Forget();
        }

        public void Hide(bool isForward = true)
        {
            HideAsync(isForward).Forget();
        }

        public void ShowImmediate()
        {
            isActive = true;
            
            pageAnimator.SetBool(IsActive, true);
            pageAnimator.EvaluateCurrentClipEndAsync().Forget();
            
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            if (selectOnShow)
            {
                TrySelectDefaultSelectable();
            }
            
            OnShown?.Invoke();
        }
        
        public void HideImmediate()
        {
            OnHidden?.Invoke();
            
            if (EventSystem.current.currentSelectedGameObject != null &&
                EventSystem.current.currentSelectedGameObject.transform.IsChildOf(transform))
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
            
            isActive = false;
            
            pageAnimator.SetBool(IsActive, false);
            pageAnimator.EvaluateCurrentClipEndAsync().Forget();
            
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        public void SetDefaultSelectable(Selectable selectable)
        {
            defaultSelectable = selectable;
        }
        
        public void SetInteractable(bool isInteractable)
        {
            canvasGroup.interactable = isInteractable;
            canvasGroup.blocksRaycasts = isInteractable;
        }

        private void TrySelectDefaultSelectable()
        {
            if (hasDefaultSelectable && canvasGroup.interactable && InputManager.CurrentControlScheme is not ControlScheme.Mouse)
            {
                EventSystem.current.SetSelectedGameObject(null);
                defaultSelectable.Select();
            }
        }
        
        private void OnDestroy()
        {
            InputManager.OnControlSchemeChanged -= HandleControlSchemeChanged;
        }
    }
}
