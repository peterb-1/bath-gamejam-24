using Cysharp.Threading.Tasks;
using UI;
using UnityEngine;

namespace Core
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] 
        private PageGroup pageGroup;

        [SerializeField] 
        private Animator animator;

        [SerializeField] 
        private float animationSpeedMultiplier;

        private void Awake()
        {
            animator.speed = animationSpeedMultiplier;
        }

        public async UniTask ShowAsync()
        {
            await pageGroup.ShowGroupAsync();
        }
        
        public async UniTask HideAsync()
        {
            await pageGroup.HideGroupAsync();
        }
    }
}
