using UnityEngine;
using UnityEngine.Rendering;

namespace Gameplay.Camera
{
    public class PostProcessOverrider : MonoBehaviour
    {
        [SerializeField] 
        private Volume volume;

        [SerializeField] 
        private VolumeProfile[] districtVolumeProfiles;

        public void SetPostProcessOverride(int district)
        {
            volume.profile = districtVolumeProfiles[district - 1];
        }

        public void RemovePostProcessOverride()
        {
            volume.profile = null;
        }
    }
}