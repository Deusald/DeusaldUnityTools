using UnityEngine;

namespace DeusaldUnityTools
{
    public interface IPoolManager
    {
        public void InitPool<T>(T prefab, int count) where T : Component, IPooledObject;

        public T GetFromPool<T>(T prefab, Transform parent = null) where T : Component, IPooledObject;

        public void ReturnToPool<T>(T pooledObject) where T : Component, IPooledObject;
    }
}