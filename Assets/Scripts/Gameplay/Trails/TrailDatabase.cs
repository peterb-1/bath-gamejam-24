using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

namespace Gameplay.Trails
{
    [CreateAssetMenu(fileName = "TrailDatabase", menuName = "Scriptable Objects/TrailDatabase")]
    public class TrailDatabase : ScriptableObject
    {
        [SerializeField] 
        private Trail[] trails;

        [field: SerializeField, Dropdown(nameof(GetTrailDropdown))] 
        public string DefaultTrail { get; private set; }

        public bool TryGetTrail(string guid, out Trail trail)
        {
            foreach (var t in trails)
            {
                if (t.Guid == guid)
                {
                    trail = t;
                    return true;
                }
            }

            trail = null;
            return false;
        }
        
        public DropdownList<string> GetTrailDropdown()
        {
            var dropdown = new DropdownList<string>();

            foreach (var trail in trails)
            {
                dropdown.Add($"{trail.Name} ({trail.Guid})", trail.Guid);
            }

            return dropdown;
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            RefreshGuids();
        }

        [Button("Refresh GUIDs")]
        private void RefreshGuids()
        {
            var guids = new HashSet<string>();
            
            foreach (var trail in trails)
            {
                if (string.IsNullOrWhiteSpace(trail.Guid) || guids.Contains(trail.Guid))
                {
                    var guid = Guid.NewGuid();
                    
                    trail.SetGuid(Guid.NewGuid());
                    guids.Add(guid.ToString());
                }
                else
                {
                    guids.Add(trail.Guid);
                }
            }
            
            EditorUtility.SetDirty(this);
        }
#endif
    }
}