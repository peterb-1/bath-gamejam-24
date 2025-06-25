using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CollectibleUIBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private Image collectibleImage;
        
        [SerializeField] 
        private Color collectibleNotFoundColour;
        
        [SerializeField]
        private Material collectibleNotFoundMaterial;
        
        [SerializeField] 
        private Color collectibleFoundColour;

        [SerializeField]
        private Material collectibleFoundMaterial;
        
        public void SetCollected(bool isCollected)
        {
            collectibleImage.material = isCollected
                ? collectibleFoundMaterial
                : collectibleNotFoundMaterial;

            collectibleImage.color = isCollected
                ? collectibleFoundColour
                : collectibleNotFoundColour;
        }
    }
}