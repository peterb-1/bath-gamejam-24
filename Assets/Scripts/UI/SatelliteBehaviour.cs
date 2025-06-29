using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SatelliteBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private Animator animator;
        
        [SerializeField] 
        private Image image;

        [SerializeField]
        private Sprite[] panelSprites;
        
        [SerializeField]
        private Sprite unlockedSprite;

        private static readonly int Disappear = Animator.StringToHash("Disappear");

        public void Disable()
        {
            gameObject.SetActive(false);
        }

        public void AnimateOut()
        {
            image.sprite = unlockedSprite;
            animator.SetTrigger(Disappear);
        }

        public void SetPanelProgress(int panels)
        {
            if (panels < panelSprites.Length)
            {
                image.sprite = panelSprites[panels];
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}