using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using Utils;

namespace UI
{
    public class PageGroup : MonoBehaviour
    {
        [SerializeField] 
        private Page[] pages;

        [SerializeField] 
        private bool activateOnAwake;
        
        [SerializeField, ShowIf(nameof(activateOnAwake))] 
        private Page initialPage;
            
        [SerializeField] 
        private Animator pageGroupAnimator;
        
        private Page activePage;
        
        private static readonly int IsActive = Animator.StringToHash("isActive");

        private void Awake()
        {
            foreach (var page in pages)
            {
                page.HideImmediate();
            }
            
            if (activateOnAwake)
            {
                ShowGroupImmediate();
                
                activePage = initialPage;
                activePage.ShowImmediate();
            }
        }
        
        public void ShowGroup()
        {
            pageGroupAnimator.SetBool(IsActive, true);
        }

        public void HideGroup()
        {
            pageGroupAnimator.SetBool(IsActive, false);
        }

        public void ShowGroupImmediate()
        {
            pageGroupAnimator.SetBool(IsActive, true);
            pageGroupAnimator.EvaluateCurrentClipEndAsync().Forget();
        }
        
        public void HideGroupImmediate()
        {
            pageGroupAnimator.SetBool(IsActive, false);
            pageGroupAnimator.EvaluateCurrentClipEndAsync().Forget();
        }

        public void SetPage(Page page)
        {
            activePage.Hide();
            activePage = page;
            activePage.Show();
        }
    }
}