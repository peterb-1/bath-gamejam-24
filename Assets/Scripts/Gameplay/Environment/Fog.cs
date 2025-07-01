using Core.Saving;
using UnityEngine;

namespace Gameplay.Environment
{
    public class Fog : MonoBehaviour
    {
        [SerializeField] 
        private SpriteRenderer spriteRenderer;
        
        [SerializeField] 
        private Material highQualityMaterial;

        [SerializeField] 
        private Color highQualityColour;

        private void Awake()
        {
            if (SaveManager.Instance.SaveData.PreferenceData.TryGetValue(SettingId.FogQuality, out FogQuality fogQuality) &&
                fogQuality is FogQuality.High)
            {
                spriteRenderer.material = highQualityMaterial;
                spriteRenderer.color = highQualityColour;
            }
        }
    }
}