using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

namespace DeusaldUnityTools
{
    public abstract class PoolManagerBase : MonoBehaviour, IPoolManager
    {
        [SerializeField] private int _PoolInitSize = 1;

        private readonly Dictionary<Component, Queue<Component>> _Pools            = new();
        private readonly Dictionary<Component, Component>        _InstanceToPrefab = new();

        public void InitPool<T>(T prefab, int count) where T : Component, IPooledObject
        {
            Queue<Component> pool = _Pools.GetValueOrDefault(prefab);
            if (pool == null)
            {
                pool = new Queue<Component>();
                _Pools.Add(prefab, pool);
            }

            for (int i = 0; i < count - pool.Count; i++)
            {
                Component pooledObject = InnerInstantiate(prefab, transform).GetComponent<T>();
                pooledObject.gameObject.SetActive(false);
                pool.Enqueue(pooledObject);
            }
        }

        public T GetFromPool<T>(T prefab, Transform parent = null) where T : Component, IPooledObject
        {
            Queue<Component> pool = _Pools.GetValueOrDefault(prefab);
            if (pool == null)
            {
                InitPool(prefab, _PoolInitSize);
                pool = _Pools[prefab];
            }

            if (!pool.TryDequeue(out Component instance))
            {
                instance = InnerInstantiate(prefab.gameObject, transform).GetComponent<T>();
            }

            _InstanceToPrefab.Add(instance, prefab);
            T instanceAsT = instance as T;
            instanceAsT!.transform.parent = parent != null ? parent : transform;
            instanceAsT.gameObject.SetActive(true);
            instanceAsT.OnGetFromPool();
            return instanceAsT;
        }

        public void ReturnToPool<T>(T pooledObject) where T : Component, IPooledObject
        {
            if (pooledObject != null)
            {
                pooledObject.OnReturnToPool();
                pooledObject.transform.parent = transform;
                pooledObject.gameObject.SetActive(false);
                Component prefab = _InstanceToPrefab.GetValueOrDefault(pooledObject);
                if (prefab != null)
                {
                    _InstanceToPrefab.Remove(pooledObject);
                    _Pools[prefab].Enqueue(pooledObject);
                }
                else
                {
                    Destroy(pooledObject.gameObject);
                }
            }
        }

        public void ClearPools()
        {
            foreach (Queue<Component> pool in _Pools.Values)
            {
                pool.ForEach(o => Destroy(o.gameObject));
            }

            _Pools.Clear();
            _InstanceToPrefab.Clear();
        }

        // Instantiate is an abstract methods because it can be implemented using DI for example
        protected abstract GameObject InnerInstantiate(Object prefab, Transform parent);
    }
}