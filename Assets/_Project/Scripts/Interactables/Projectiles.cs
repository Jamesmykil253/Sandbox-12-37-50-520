using UnityEngine;
using Photon.Pun; // **NEW**: Added for PUN 2 functionality

namespace Platformer
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 15f;
        [SerializeField] private float lifetime = 3f;

        private int _damage;
        private CharacterStats _owner;
        private bool _isEmpowered;
        private LayerMask _targetLayerMask;
        private PhotonView _photonView;

        private void Awake()
        {
            _photonView = GetComponent<PhotonView>();
        }

        public void Initialize(CharacterStats owner, int damage, bool isEmpowered, LayerMask targetLayerMask)
        {
            _owner = owner;
            _damage = damage;
            _isEmpowered = isEmpowered;
            _targetLayerMask = targetLayerMask;
            
            // Only the master client should manage the lifetime destruction to prevent conflicts
            if(PhotonNetwork.IsMasterClient)
            {
                Destroy(gameObject, lifetime);
            }
        }

        private void Update()
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            // The projectile's logic should only be executed by the master client to ensure authoritative hit detection.
            if (!PhotonNetwork.IsMasterClient) return;

            if ((_targetLayerMask.value & (1 << other.gameObject.layer)) > 0)
            {
                if (other.TryGetComponent<Hurtbox>(out var hurtbox))
                {
                    if (hurtbox.statsController != _owner && hurtbox.statsController.team != _owner.team)
                    {
                        Debug.Log($"{_owner.name}'s projectile hit {hurtbox.statsController.name}!");
                        
                        // **REFACTORED** with null check protection
                        PhotonView targetView = hurtbox.statsController.GetComponent<PhotonView>();
                        PhotonView ownerView = _owner.GetComponent<PhotonView>();
                        if(targetView != null && ownerView != null)
                        {
                            targetView.RPC("Rpc_TakeDamage", RpcTarget.All, _damage, ownerView.ViewID);
                        }
                        
                        PhotonNetwork.Destroy(gameObject); // Use PhotonNetwork.Destroy for networked objects
                    }
                }
            }
        }
    }
}