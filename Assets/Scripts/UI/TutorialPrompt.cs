using UnityEngine;

namespace UI
{
    public class TutorialPrompt : MonoBehaviour
    {
        [SerializeField] 
        private Animator animator;

        [SerializeField] 
        private Collider2D entryCollider;
        
        [SerializeField] 
        private Collider2D exitCollider;
        
        [SerializeField]
        private LayerMask playerLayers;

        private bool hasActivated;
        private bool hasDeactivated;
        
        private static readonly int IsActive = Animator.StringToHash("isActive");

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((playerLayers.value & (1 << other.gameObject.layer)) != 0)
            {
                if (entryCollider.IsTouching(other))
                {
                    TryActivate();
                }

                if (exitCollider.IsTouching(other))
                {
                    TryDeactivate();
                }
            }
        }

        private void TryActivate()
        {
            if (hasActivated) return;

            hasActivated = true;
            
            animator.SetBool(IsActive, true);
        }
        
        private void TryDeactivate()
        {
            if (hasDeactivated || !hasActivated) return;

            hasDeactivated = true;
            
            animator.SetBool(IsActive, false);
        }
    }
}
