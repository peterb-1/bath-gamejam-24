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
        
        private Page activePage;
        
        private static readonly int IsActive = Animator.StringToHash("isActive");

        private void Awake()
        {
            activePage = initialPage;
            
            foreach (var page in pages)
            {
                page.HideImmediate();
            }
            
            if (activateOnAwake)
            {
                ShowGroupImmediate();
            }
        }

        public async UniTask ShowGroupAsync()
        {
            pageGroupAnimator.SetBool(IsActive, true);
            
            activePage.Show();

            var duration = await pageGroupAnimator.GetCurrentClipDurationAsync();
            await UniTask.Delay(TimeSpan.FromSeconds(duration));

            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        public async UniTask HideGroupAsync()
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            activePage.Hide();
            
            pageGroupAnimator.SetBool(IsActive, false);

            var duration = await pageGroupAnimator.GetCurrentClipDurationAsync();
            await UniTask.Delay(TimeSpan.FromSeconds(duration));
        }
        
        public void ShowGroup()
        {
            ShowGroupAsync().Forget();
        }

        public void HideGroup()
        {
            HideGroupAsync().Forget();
        }

        public void ShowGroupImmediate()
        {
            pageGroupAnimator.SetBool(IsActive, true);
            pageGroupAnimator.EvaluateCurrentClipEndAsync().Forget();
            
            activePage.ShowImmediate();
            
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        public void HideGroupImmediate()
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            activePage.HideImmediate();
            
            pageGroupAnimator.SetBool(IsActive, false);
            pageGroupAnimator.EvaluateCurrentClipEndAsync().Forget();
        }

        public void SetPage(Page page, bool isForward = true)
        {
            if (page == activePage) return;
            
            if (!pages.Contains(page))
            {
                GameLogger.LogWarning($"Page {page} is not registered to this PageGroup - cannot set as the active page.", this);
                return;
            }
            
            activePage.Hide(isForward);
            activePage = page;
            activePage.Show(isForward);
        }
    }
}
