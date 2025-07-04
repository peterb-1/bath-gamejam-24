using System;
using Core.Saving;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Gameplay.Camera
{
    public class GraphicSettingsUpdater : MonoBehaviour
    {
        [SerializeField] 
        private UniversalRenderPipelineAsset urpAsset;

        private async void Awake()
        {
            await UniTask.WaitUntil(() => SaveManager.IsReady);

            SaveManager.Instance.SaveData.PreferenceData.OnSettingChanged += HandleSettingChanged;
            
            InitialiseSetting(SettingId.RenderScale);
            InitialiseSetting(SettingId.Fullscreen);
            InitialiseSetting(SettingId.VSync);
        }
        
        private void InitialiseSetting(SettingId settingId)
        {
            if (SaveManager.Instance.SaveData.PreferenceData.TryGetValue(settingId, out object value))
            {
                HandleSettingChanged(settingId, value);
            }
        }

        private void HandleSettingChanged(SettingId settingId, object value)
        {
            switch (settingId)
            {
                case SettingId.RenderScale:
                    if (value is RenderScale renderScale)
                    {
                        urpAsset.renderScale = renderScale switch
                        {
                            RenderScale.Low => 0.9f,
                            RenderScale.Medium => 1.0f,
                            RenderScale.High => 1.25f,
                            RenderScale.Ultra => 1.5f,
                            _ => throw new ArgumentOutOfRangeException()
                        };
                    }
                    break;
                case SettingId.Fullscreen:
                    if (value is bool isFullscreen)
                    {
                        Screen.fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
                    }
                    break;
                case SettingId.VSync:
                    if (value is bool isVSyncEnabled)
                    {
                        QualitySettings.vSyncCount = isVSyncEnabled ? 1 : 0;
                    }
                    break;
            }
        }
    }
}