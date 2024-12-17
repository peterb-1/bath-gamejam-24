using UnityEngine;

namespace Gameplay.Colour
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
