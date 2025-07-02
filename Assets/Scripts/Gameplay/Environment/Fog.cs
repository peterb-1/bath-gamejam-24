using Core.Saving;
using UnityEngine;

namespace Gameplay.Environment
{
    public class Fog : MonoBehaviour
    {
        [SerializeField] 
        private SpriteRenderer spriteRenderer;
        
        [SerializeField] 
        private Sprite highQualitySprite;
        
        [SerializeField] 
        private Material highQualityMaterial;

        [SerializeField] 
        private Color highQualityColour;
        
        [SerializeField] 
        private Material mediumQualityMaterial;

        private void Awake()
        {
            if (!SaveManager.Instance.SaveData.PreferenceData.TryGetValue(SettingId.FogQuality, out FogQuality fogQuality)) return;
            
            switch (fogQuality)
            {
                case FogQuality.Medium:
                    spriteRenderer.material = mediumQualityMaterial;
                    break;
                case FogQuality.High:
                    spriteRenderer.sprite = highQualitySprite;
                    spriteRenderer.material = highQualityMaterial;
                    spriteRenderer.color = highQualityColour;
                    break;
            }
        }
    }
}