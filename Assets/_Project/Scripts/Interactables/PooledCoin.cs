// File: Assets/_Project/Scripts/Interactables/PooledCoin.cs
// Fix: Explicitly specified Platformer.Core.Logger to resolve ambiguity.

using UnityEngine;
using System.Collections;

namespace Platformer
{
    public class PooledCoin : MonoBehaviour, Core.IPoolable
    {
        public float spinSpeed = 90f;
        public int coinValue = 1;
        public float despawnTime = 30f;
        
        private Coroutine _despawnCoroutine;
        
        public void Initialize(int value, Vector3 position, float despawnDelay = 30f)
        {
            coinValue = value;
            transform.position = position;
            
            if (despawnDelay > 0)
            {
                if (_despawnCoroutine != null) StopCoroutine(_despawnCoroutine);
                _despawnCoroutine = StartCoroutine(DespawnTimer(despawnDelay));
            }
            
            Platformer.Core.Logger.Debug(Platformer.Core.Logger.LogCategory.Scoring, $"Coin initialized with value {value} at {position}", this);
        }
        
        private void Update()
        {
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
        }
        
        private IEnumerator DespawnTimer(float delay)
        {
            yield return new WaitForSeconds(delay);
            Platformer.Core.Logger.Debug(Platformer.Core.Logger.LogCategory.Scoring, "Coin despawned due to timeout", this);
            ReturnToPool();
        }
        
        public void Collect()
        {
            Platformer.Core.Logger.Debug(Platformer.Core.Logger.LogCategory.Scoring, $"Coin collected for {coinValue} points", this);
            ReturnToPool();
        }
        
        private void ReturnToPool()
        {
            if (Core.PoolManager.Instance != null)
            {
                Core.PoolManager.Instance.ReturnCoin(this);
            }
            else
            {
                Platformer.Core.Logger.Warning(Platformer.Core.Logger.LogCategory.Performance, "No PoolManager found, destroying coin", this);
                Destroy(gameObject);
            }
        }

        public void OnPoolGet()
        {
            Platformer.Core.Logger.Verbose(Platformer.Core.Logger.LogCategory.Performance, "Coin retrieved from pool", this);
        }

        public void OnPoolReturn()
        {
            if (_despawnCoroutine != null)
            {
                StopCoroutine(_despawnCoroutine);
                _despawnCoroutine = null;
            }
            Platformer.Core.Logger.Verbose(Platformer.Core.Logger.LogCategory.Performance, "Coin returned to pool", this);
        }
    }
}