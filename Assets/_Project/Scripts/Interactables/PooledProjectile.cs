// File: Assets/_Project/Scripts/Interactables/PooledProjectile.cs
// Fix: Wrapped damage RPC call in a network-aware check.

using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace Platformer
{
    public class PooledProjectile : MonoBehaviour, Core.IPoolable
    {
        // ... (Fields are unchanged) ...
        [SerializeField] private float speed = 15f;
        [SerializeField] private float lifetime = 3f;
        private int _damage;
        private CharacterStats _owner;
        private LayerMask _targetLayerMask;
        private Coroutine _lifetimeCoroutine;
        private Vector3 _direction;

        // ... (Initialize, Update, LifetimeCountdown methods are unchanged) ...
        public void Initialize(CharacterStats owner, int damage, bool isEmpowered, LayerMask targetLayerMask, Vector3 direction)
        {
            _owner = owner;
            _damage = damage;
            _targetLayerMask = targetLayerMask;
            _direction = direction.normalized;
            transform.rotation = Quaternion.LookRotation(_direction);

            if (_lifetimeCoroutine != null) StopCoroutine(_lifetimeCoroutine);
            _lifetimeCoroutine = StartCoroutine(LifetimeCountdown());
            Platformer.Core.Logger.Debug(Platformer.Core.Logger.LogCategory.Combat, $"Projectile initialized - Damage: {damage}, Empowered: {isEmpowered}", this);
        }

        private void Update()
        {
            transform.Translate(_direction * speed * Time.deltaTime, Space.World);
        }

        private IEnumerator LifetimeCountdown()
        {
            yield return new WaitForSeconds(lifetime);
            ReturnToPool();
        }


        private void OnTriggerEnter(Collider other)
        {
            if (PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient) return;

            if ((_targetLayerMask.value & (1 << other.gameObject.layer)) > 0)
            {
                if (other.TryGetComponent<Hurtbox>(out var hurtbox))
                {
                    if (hurtbox.statsController != _owner && hurtbox.statsController.team != _owner.team)
                    {
                        PhotonView targetView = hurtbox.statsController.GetComponent<PhotonView>();
                        PhotonView ownerView = _owner.GetComponent<PhotonView>();
                        
                        if (targetView != null && ownerView != null)
                        {
                            // THE FIX IS HERE
                            if (PhotonNetwork.InRoom)
                            {
                                targetView.RPC("Rpc_TakeDamage", RpcTarget.All, _damage, ownerView.ViewID);
                            }
                            else
                            {
                                hurtbox.statsController.Rpc_TakeDamage(_damage, ownerView.ViewID);
                            }
                        }
                        
                        ReturnToPool();
                    }
                }
            }
        }
        
        // ... (ReturnToPool and IPoolable methods are unchanged) ...
        private void ReturnToPool()
        {
            if (Core.PoolManager.Instance != null) { Core.PoolManager.Instance.ReturnProjectile(this); }
            else { Destroy(gameObject); }
        }

        public void OnPoolGet() { }
        public void OnPoolReturn()
        {
            if (_lifetimeCoroutine != null) { StopCoroutine(_lifetimeCoroutine); _lifetimeCoroutine = null; }
            _owner = null;
        }
    }
}