using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils;

namespace UI
{
    public class Page : MonoBehaviour
    {
        [SerializeField] 
        private Animator pageAnimator;

        private static readonly int IsActive = Animator.StringToHash("isActive");
        private static readonly int IsForward = Animator.StringToHash("isActive");

        public void Show(bool isForward = true)
        {
            pageAnimator.SetBool(IsForward, isForward);
            pageAnimator.SetBool(IsActive, true);
        }

        public void Hide(bool isForward = true)
        {
            pageAnimator.SetBool(IsForward, isForward);
            pageAnimator.SetBool(IsActive, false);
        }

        public void ShowImmediate()
        {
            pageAnimator.SetBool(IsActive, true);
            pageAnimator.EvaluateCurrentClipEndAsync().Forget();
        }
        
        public void HideImmediate()
        {
            pageAnimator.SetBool(IsActive, false);
            pageAnimator.EvaluateCurrentClipEndAsync().Forget();
        }
    }
}
