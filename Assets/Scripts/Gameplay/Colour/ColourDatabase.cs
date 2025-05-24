using UnityEngine;

namespace Gameplay.Colour
{
    [CreateAssetMenu(fileName = "ColourDatabase", menuName = "Scriptable Objects/ColourDatabase")]
    public class ColourDatabase : ScriptableObject
    {
        [SerializeField] 
        private ColourConfig[] colourConfigs;

        [SerializeField] 
        private DistrictColourOverride[] districtColourOverrides;

        public bool TryGetColourConfig(ColourId colourId, out ColourConfig colourConfig, int district = -1)
        {
            if (district > 0)
            {
                foreach (var colourOverride in districtColourOverrides)
                {
                    if (colourOverride.District == district && colourOverride.ConfigOverride.ColourId == colourId)
                    {
                        colourConfig = colourOverride.ConfigOverride;
                        return true;
                    }
                }
            }

            foreach (var config in colourConfigs)
            {
                if (config.ColourId == colourId)
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
