using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Utils
{
    public static class AnimatorUtils
    {
        public static async UniTask EvaluateCurrentClipEndAsync(this Animator animator)
        {
            // let the animator switch to the new clip - this function is likely to be called immediately after setting the animator state
            await UniTask.DelayFrame(1, delayTiming: PlayerLoopTiming.PostLateUpdate);
            
            // may have been destroyed in the delay
            if (animator != null)
            {
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                animator.Play(stateInfo.fullPathHash, -1, 1f);
            }
        }

        public static async UniTask<float> GetCurrentClipDurationAsync(this Animator animator)
        {
            // let the animator switch to the new clip - this function is likely to be called immediately after setting the animator state
            await UniTask.DelayFrame(1, delayTiming: PlayerLoopTiming.PostLateUpdate);
            
            // may have been destroyed in the delay
            if (animator != null)
            {
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                return stateInfo.length;
            }

            return 0f;
        }
    }
}
