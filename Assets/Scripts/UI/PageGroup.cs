using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils;

namespace UI
{
    public class PageGroup : MonoBehaviour
    {
        [SerializeField] 
        private bool activateOnAwake;
        
        [SerializeField] 
        private CanvasGroup canvasGroup;
        
        [SerializeField] 
        private Page[] pages;

        [SerializeField] 
        private Page initialPage;

        [SerializeField] 
        private Animator pageGroupAnimator;
        
        public Page ActivePage { get; private set; }
        
        private static readonly int IsActive = Animator.StringToHash("isActive");

        private void Awake()
        {
            ActivePage = initialPage;
            
            foreach (var page in pages)
            {
                page.HideImmediate();
            }
            
            if (activateOnAwake)
            {
                ShowGroupImmediate();
            }
            else
            {
                HideGroupImmediate();
            }
        }

        public async UniTask ShowGroupAsync(bool isForward = true)
        {
            pageGroupAnimator.SetBool(IsActive, true);
            
            ActivePage.Show(isForward);

            var duration = await pageGroupAnimator.GetCurrentClipDurationAsync();
            await UniTask.Delay(TimeSpan.FromSeconds(duration));

            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        public async UniTask HideGroupAsync(bool isForward = true)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            ActivePage.Hide(isForward);
            
            pageGroupAnimator.SetBool(IsActive, false);

            var duration = await pageGroupAnimator.GetCurrentClipDurationAsync();
            await UniTask.Delay(TimeSpan.FromSeconds(duration));
        }
        
        public void ShowGroup(bool isForward = true)
        {
            ShowGroupAsync(isForward).Forget();
        }

        public void HideGroup(bool isForward = true)
        {
            HideGroupAsync(isForward).Forget();
        }

        public void SetInteractable(bool isInteractable)
        {
            canvasGroup.interactable = isInteractable;
            canvasGroup.blocksRaycasts = isInteractable;
        }

        public void ShowGroupImmediate()
        {
            pageGroupAnimator.SetBool(IsActive, true);
            pageGroupAnimator.EvaluateCurrentClipEndAsync().Forget();
            
            ActivePage.ShowImmediate();
            
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        public void HideGroupImmediate()
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            ActivePage.HideImmediate();
            
            pageGroupAnimator.SetBool(IsActive, false);
            pageGroupAnimator.EvaluateCurrentClipEndAsync().Forget();
        }

        public async UniTask SetPageAsync(Page page, bool isForward = true)
        {
            if (page == ActivePage) return;
            
            if (!pages.Contains(page))
            {
                GameLogger.LogWarning($"Page {page} is not registered to this PageGroup - cannot set as the active page.", this);
                return;
            }
            
            ActivePage.Hide(isForward);
            ActivePage = page;
            
            await ActivePage.ShowAsync(isForward);
        }

        public void SetPage(Page page, bool isForward = true)
        {
            SetPageAsync(page, isForward).Forget();
        }

        public void SetDefaultPage(bool isForward = true)
        {
            SetPageAsync(initialPage, isForward).Forget();
        }
    }
}
