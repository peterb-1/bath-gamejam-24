using Core.Saving;
using NaughtyAttributes;
using UnityEngine;

namespace Gameplay.Environment
{
    public class Cloud : MonoBehaviour
    {
        [SerializeField] 
        private SpriteRenderer spriteRenderer;

        [SerializeField] 
        private Material fancyMaterial;

        [SerializeField] 
        private Material cheapMaterial;
        
        [SerializeField, ReadOnly]
        private float horizontalSpeed;
        
        [SerializeField, ReadOnly]
        private float leftEnd;
        
        [SerializeField, ReadOnly]
        private float rightEnd;

        private void Awake()
        {
            SaveManager.Instance.SaveData.PreferenceData.OnSettingChanged += HandleSettingChanged;
            
            InitialiseSetting(SettingId.FancyClouds);
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
            if (settingId is SettingId.FancyClouds && value is bool areFancyCloudsEnabled)
            {
                spriteRenderer.material = areFancyCloudsEnabled ? fancyMaterial : cheapMaterial;
            }
        }

        public void Configure(float speed, float left, float right)
        {
            horizontalSpeed = speed;
            leftEnd = left;
            rightEnd = right;
        }

        private void Update()
        {
            var position = transform.localPosition;
            
            position += Vector3.right * (horizontalSpeed * Time.deltaTime);

            if (horizontalSpeed > 0 && position.x > rightEnd)
            {
                position.x -= rightEnd - leftEnd;
            }
            else if (horizontalSpeed < 0 && position.x < leftEnd)
            {
                position.x += rightEnd - leftEnd;
            }

            transform.localPosition = position;
        }

        private void OnDestroy()
        {
            SaveManager.Instance.SaveData.PreferenceData.OnSettingChanged -= HandleSettingChanged;
        }
    }
}
