using UnityEngine;

namespace Core.Saving
{
    [CreateAssetMenu(menuName = "Settings/RenderScaleSetting")]
    public class GraphicsQualitySetting : AbstractSetting<RenderScale> {}
    
    public enum RenderScale
    {
        Low,
        Medium,
        High,
        Ultra
    }
}