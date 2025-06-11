using System;
using Gameplay.Trails;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

namespace Gameplay.Achievements
{
    [CreateAssetMenu(fileName = "Achievement", menuName = "Scriptable Objects/Achievement")]
    public class Achievement : ScriptableObject
    {
        [field: SerializeField, ReadOnly] 
        public string Guid { get; private set; }
        
        [field: SerializeField] 
        public string Name { get; private set; }
        
        [field: SerializeField] 
        public string SteamName { get; private set; }

        [field: SerializeField] 
        public string UnlockDescription { get; private set; }

        [field: SerializeField, Dropdown(nameof(GetTrailGuids)), OnValueChanged(nameof(OnTrailChanged))]
        public string TrailGuid { get; private set; }

        private void OnTrailChanged()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
        
        private DropdownList<string> GetTrailGuids()
        {
#if UNITY_EDITOR
            var guids = AssetDatabase.FindAssets($"t:{nameof(TrailDatabase)}");
            var assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            var trailDatabase = AssetDatabase.LoadAssetAtPath<TrailDatabase>(assetPath);

            return trailDatabase.GetTrailDropdown();
#else
            return new DropdownList<string>();
#endif
        }

#if UNITY_EDITOR
        public void SetGuid(Guid guid)
        {
            Guid = guid.ToString();
        }
#endif
    }
}