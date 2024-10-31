using UnityEngine;

namespace Core.Colour
{
    [CreateAssetMenu(fileName = "ColourDatabase", menuName = "Scriptable Objects/ColourDatabase")]
    public class ColourDatabase : ScriptableObject
    {
        [SerializeField] 
        private ColourConfig[] colourConfigs;

        public bool TryGetColourConfig(ColourId colourId, out ColourConfig colourConfig)
        {
            foreach (var config in colourConfigs)
            {
                if (config.colourId == colourId)
                {
                    colourConfig = config;
                    return true;
                }
            }

            colourConfig = null;
            return false;
        }
    }
}
