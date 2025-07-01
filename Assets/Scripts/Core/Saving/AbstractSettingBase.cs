using System;
using UnityEngine;

namespace Core.Saving
{
    public abstract class AbstractSettingBase : ScriptableObject 
    {
        [field: SerializeField] 
        public SettingId SettingId { get; private set; }
        
        [field: SerializeField] 
        public string DisplayName { get; private set; }
        
        [field: SerializeField, TextArea] 
        public string Description { get; private set; }
    
        public abstract object GetDefaultValue();
        public abstract Type GetValueType();
    }
}