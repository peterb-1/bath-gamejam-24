using UnityEngine;

namespace Core.Saving
{
    [CreateAssetMenu(menuName = "Settings/FogQualitySetting")]
    public class FogQualitySetting : AbstractSetting<FogQuality> {}
    
    public enum FogQuality
    {
        Low,
        Medium,
        High
    }
}