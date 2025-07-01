using Core.Saving;
using UnityEngine;

namespace Gameplay.Environment
{
    public class Background : MonoBehaviour
    {
        [SerializeField] 
        private SpriteRenderer spriteRenderer;
        
        [SerializeField] 
        private Material lowQualityMaterial;

        private void Awake()
        {
            if (SaveManager.Instance.SaveData.PreferenceData.TryGetValue(SettingId.FogQuality, out FogQuality fogQuality) &&
                fogQuality is FogQuality.Low)
            {
                spriteRenderer.material = lowQualityMaterial;
            }
        }
    }
}