using UnityEngine;
using UnityEngine.Pool;

namespace Utils
{
    /// <summary>
    /// A wrapper class for Unity's ObjectPool
    /// </summary>
    public class Pool<T> where T : Component
    {
        private readonly ObjectPool<T> pool;
        private readonly T poolObject;
        private readonly GameObject rootGameObject;
        
        public Pool(T item, int defaultCapacity = 100)
        {
            rootGameObject = new GameObject($"POOL - {item.name}");
            
            poolObject = item;
            
            pool = new ObjectPool<T>(
                ActionOnCreate,
                ActionOnGet,
                ActionOnRelease,
                ActionOnDestroy,
                defaultCapacity: defaultCapacity
            );

            var objects = new T[defaultCapacity];

            for (var i = 0; i < defaultCapacity; i++)
            {
                objects[i] = pool.Get();
            }
            
            for (var i = 0; i < defaultCapacity; i++)
            {
                pool.Release(objects[i]);
            }
        }

        public T Get(Transform parent = null)
        {
            var gameObject = pool.Get();

            if (parent != null)
            {
                gameObject.transform.SetParent(parent, false);
            }
            
            return gameObject;
        }

        public void Release(T item)
        {
            pool.Release(item);
        }

        private T ActionOnCreate()
        {
            var item = Object.Instantiate(poolObject, rootGameObject.transform);
            item.name = $"{poolObject.name} ({pool.CountAll})";
            return item;
        }
        
        private void ActionOnGet(T item)
        {
            item.gameObject.SetActive(true);
        }
        
        private void ActionOnRelease(T item)
        {
            item.gameObject.transform.SetParent(rootGameObject.transform, false);
            item.gameObject.SetActive(false);
        }
        
        private static void ActionOnDestroy(T item)
        {
            Object.Destroy(item.gameObject);
        }
    }
}
