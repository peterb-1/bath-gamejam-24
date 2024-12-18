using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RankingStar : MonoBehaviour
    {
        [SerializeField] 
        private Animator animator;
        
        [SerializeField] 
        private Image outlineImage;
        
        [SerializeField] 
        private Image centreImage;

        [SerializeField] 
        private Material rainbowOutlineMaterial;

        [SerializeField] 
        private Material rainbowCentreMaterial;

        private static readonly int IsActive = Animator.StringToHash("IsActive");
        private static readonly int Flash = Animator.StringToHash("Flash");

        public void SetRainbowState(bool isRainbow, bool shouldAnimate = true)
        {
            outlineImage.material = isRainbow ? rainbowOutlineMaterial : null;
            centreImage.material = isRainbow ? rainbowCentreMaterial : null;

            if (shouldAnimate)
            {
                animator.SetTrigger(Flash);
            }
        }

        public void SetActive(bool isActive, bool shouldAnimate = true)
        {
            if (shouldAnimate)
            {
                animator.SetBool(IsActive, isActive);
            }
            else
            {
                centreImage.gameObject.SetActive(isActive);
            }
        }
    }
}