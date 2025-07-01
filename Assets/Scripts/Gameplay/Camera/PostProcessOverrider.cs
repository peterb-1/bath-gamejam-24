using Core.Saving;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Gameplay.Camera
{
    public class PostProcessOverrider : MonoBehaviour
    {
        [SerializeField] 
        private Volume volume;

        [SerializeField] 
        private VolumeProfile defaultProfile;

        [SerializeField] 
        private VolumeProfile[] districtVolumeProfiles;

        private async void Awake()
        {
            await UniTask.WaitUntil(() => SaveManager.IsReady);

            SaveManager.Instance.SaveData.PreferenceData.OnSettingChanged += HandleSettingChanged;
            
            ApplySettingsToProfile();
        }

        private void HandleSettingChanged(SettingId settingId, object _)
        {
            if (settingId is SettingId.Gamma)
            {
                ApplySettingsToProfile();
            }
        }

        public void SetPostProcessOverride(int district)
        {
            volume.profile = districtVolumeProfiles[district - 1];

            ApplySettingsToProfile();
        }

        public void RemovePostProcessOverride()
        {
            volume.profile = defaultProfile;
            
            ApplySettingsToProfile();
        }

        private void ApplySettingsToProfile()
        {
            if (SaveManager.Instance.SaveData.PreferenceData.TryGetValue(SettingId.Gamma, out float gamma) &&
                volume.profile != null &&
                volume.profile.TryGet<LiftGammaGain>(out var liftGammaGain))
            {
                liftGammaGain.gamma.overrideState = true;
                liftGammaGain.gamma.value = new Vector4(gamma, gamma, gamma, gamma);
            }
        }

        private void OnDestroy()
        {
            SaveManager.Instance.SaveData.PreferenceData.OnSettingChanged -= HandleSettingChanged;
        }
    }
}