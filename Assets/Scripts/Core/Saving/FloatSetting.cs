using UnityEngine;

namespace Core.Saving
{
    [CreateAssetMenu(menuName = "Settings/FloatSetting")]
    public class FloatSetting : AbstractSetting<float>
    {
        [field: SerializeField]
        public float Min { get; private set; }
        
        [field: SerializeField]
        public float Max { get; private set; }
    }
}