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
            
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            animator.Play(stateInfo.fullPathHash, -1, 1f);
        }
    }
}