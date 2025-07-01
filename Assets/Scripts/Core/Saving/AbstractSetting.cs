using System;
using UnityEngine;

namespace Core.Saving
{
    public abstract class AbstractSetting<T> : AbstractSettingBase 
    {
        [field: SerializeField] 
        public T DefaultValue { get; private set; }
    
        public override object GetDefaultValue() => DefaultValue;
        public override Type GetValueType() => typeof(T);
    }
}