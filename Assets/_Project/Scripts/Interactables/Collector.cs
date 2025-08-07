using UnityEngine;

namespace Platformer
{
    [RequireComponent(typeof(Collider))]
    public class Collector : MonoBehaviour
    {
        private PlayerController _playerController;

        private void Awake()
        {
            _playerController = GetComponentInParent<PlayerController>();
            if (_playerController == null)
            {
                Debug.LogError("Collector could not find PlayerController in parent!", this);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<Coin>(out Coin coin))
            {
                if (_playerController != null && _playerController.Scoring != null)
                {
                    // **THE FIX**: Instead of calling playerController.CollectCoin,
                    // we now call the method on its Scoring component.
                    _playerController.Scoring.CollectCoin(coin);
                }
            }
        }
    }
}