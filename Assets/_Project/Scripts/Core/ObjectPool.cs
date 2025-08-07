// File: Assets/_Project/Scripts/Core/ObjectPool.cs
// Fix: Restored the GetStats() method and PoolStats struct.

using UnityEngine;
using System.Collections.Generic;

namespace Platformer.Core
{
    public class ObjectPool<T> where T : Component, IPoolable
    {
        private readonly Queue<T> _pool = new Queue<T>();
        private readonly GameObject _prefab;
        private readonly Transform _parent;
        private readonly int _maxSize;
        private int _createdCount = 0;
        
        public ObjectPool(GameObject prefab, int initialSize = 10, int maxSize = 50, Transform parent = null)
        {
            _prefab = prefab;
            _maxSize = maxSize;
            _parent = parent;
            
            for (int i = 0; i < initialSize; i++)
            {
                CreateNewObject(activate: false);
            }
            Logger.Info(Logger.LogCategory.Performance, $"Initialized object pool for {_prefab.name} with {initialSize} objects");
        }
        
        private T CreateNewObject(bool activate = true)
        {
            var obj = Object.Instantiate(_prefab, _parent);
            var component = obj.GetComponent<T>();
            
            if (component == null)
            {
                Logger.Error(Logger.LogCategory.General, $"Prefab {_prefab.name} does not have component {typeof(T).Name}!");
                Object.Destroy(obj);
                return null;
            }
            
            obj.SetActive(activate);
            _createdCount++;
            if(!activate) _pool.Enqueue(component);
            return component;
        }
        
        public T Get()
        {
            if (_pool.Count > 0)
            {
                T obj = _pool.Dequeue();
                obj.gameObject.SetActive(true);
                obj.OnPoolGet();
                return obj;
            }
            
            if (_createdCount < _maxSize)
            {
                T newObj = CreateNewObject();
                newObj.OnPoolGet();
                return newObj;
            }
            
            Logger.Warning(Logger.LogCategory.Performance, $"Object pool for {_prefab.name} is at capacity ({_maxSize})!");
            return null;
        }
        
        public void Return(T obj)
        {
            if (obj == null) return;
            
            obj.gameObject.SetActive(false);
            obj.OnPoolReturn();
            _pool.Enqueue(obj);
        }

        // THE FIX IS HERE: Method and struct restored.
        public PoolStats GetStats()
        {
            return new PoolStats
            {
                AvailableCount = _pool.Count,
                CreatedCount = _createdCount,
                MaxSize = _maxSize
            };
        }
    }

    public interface IPoolable
    {
        void OnPoolGet();
        void OnPoolReturn();
    }

    public struct PoolStats
    {
        public int AvailableCount;
        public int CreatedCount;
        public int MaxSize;
    }

    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }
        
        [Header("Pool Configuration")]
        public GameObject projectilePrefab;
        public GameObject coinPrefab;
        public int initialPoolSize = 20;
        public int maxPoolSize = 100;
        
        private ObjectPool<PooledProjectile> _projectilePool;
        private ObjectPool<PooledCoin> _coinPool;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (projectilePrefab != null)
            {
                _projectilePool = new ObjectPool<PooledProjectile>(projectilePrefab, initialPoolSize, maxPoolSize, transform);
            }
            if (coinPrefab != null)
            {
                _coinPool = new ObjectPool<PooledCoin>(coinPrefab, initialPoolSize, maxPoolSize, transform);
            }
            Logger.Info(Logger.LogCategory.Performance, "PoolManager initialized successfully");
        }
        
        public PooledProjectile GetProjectile() => _projectilePool?.Get();
        public void ReturnProjectile(PooledProjectile projectile) => _projectilePool?.Return(projectile);
        
        public PooledCoin GetCoin() => _coinPool?.Get();
        public void ReturnCoin(PooledCoin coin) => _coinPool?.Return(coin);

        public void LogPoolStats()
        {
            if (_projectilePool != null) Logger.Performance(Logger.LogLevel.Info, $"Projectile Pool - Available: {_projectilePool.GetStats().AvailableCount}");
            if (_coinPool != null) Logger.Performance(Logger.LogLevel.Info, $"Coin Pool - Available: {_coinPool.GetStats().AvailableCount}");
        }
    }
}