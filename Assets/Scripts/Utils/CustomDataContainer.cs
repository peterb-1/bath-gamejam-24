using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils
{
    public class CustomDataContainer
    {
        private readonly Dictionary<Type, Dictionary<object, object>> container;

        public CustomDataContainer()
        {
            container = new Dictionary<Type, Dictionary<object, object>>();
        }

        public void SetCustomData<T>(object key, T value)
        {
            var type = typeof(T);
            
            if (!container.ContainsKey(typeof(T)))
            {
                container.Add(type, new Dictionary<object, object>());
            }
            
            if (!container[type].TryAdd(key, value))
            {
                container[type][key] = value;
            }
        }

        public bool TryGetCustomData<T>(object key, out T value)
        {
            var type = typeof(T);

            if (container.TryGetValue(type, out var typeContainer))
            {
                if (typeContainer.TryGetValue(key, out var valueObject))
                {
                    value = (T) valueObject;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public bool TryGetCustomData<T>(out T value)
        {
            var type = typeof(T);

            if (container.TryGetValue(type, out var typeContainer))
            {
                if (typeContainer.Count > 0)
                {
                    value = (T) typeContainer.First().Value;
                    return true;
                }
            }

            value = default;
            return false;
        }
        
        public void RemoveCustomData<T>(object key)
        {
            var type = typeof(T);

            if (container.TryGetValue(type, out var typeContainer))
            {
                typeContainer.Remove(key);
            }
        }
    }
}
