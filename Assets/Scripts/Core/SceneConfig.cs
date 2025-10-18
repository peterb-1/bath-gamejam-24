using Gameplay.Core;
using NaughtyAttributes;
using UnityEngine;

#if UNITY_EDITOR
using System;
using UnityEditor;
#endif

namespace Core
{
    [CreateAssetMenu(fileName = "SceneConfig", menuName = "Scriptable Objects/SceneConfig")]
    public class SceneConfig : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField, OnValueChanged(nameof(OnValidate))] 
        private SceneAsset sceneAsset;
#endif
        
        [SerializeField, ReadOnly] 
        private string scenePath;

        [field: SerializeField] 
        public bool IsLevelScene { get; private set; }

        [field: SerializeField, ShowIf(nameof(IsLevelScene))]
        public LevelConfig LevelConfig { get; private set; }
        
        [field: SerializeField, ShowIf(nameof(IsLevelScene))]
        public SceneConfig[] UnlockedConfigsOnCompletion { get; private set; }

        public string ScenePath => scenePath;

#if UNITY_EDITOR
        private void OnValidate()
        {
            scenePath = sceneAsset != null ? AssetDatabase.GetAssetPath(sceneAsset) : string.Empty;

            if (string.IsNullOrWhiteSpace(LevelConfig.Guid))
            {
                RefreshGuid();
            }
        }

        [Button("Refresh GUID")]
        private void RefreshGuid()
        {
            LevelConfig.SetGuid(Guid.NewGuid());
            EditorUtility.SetDirty(this);
        }
#endif
    }
}