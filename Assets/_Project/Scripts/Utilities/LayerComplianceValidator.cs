// File: Assets/_Project/Scripts/Utilities/LayerComplianceValidator.cs
// Version: 1.2 (Compliant)
// Fix: Corrected all calls to use the full namespace 'Platformer.Core.Logger' to resolve all CS0117 errors.

using UnityEngine;

namespace Platformer.Utilities
{
    [RequireComponent(typeof(Collider))]
    public class LayerComplianceValidator : MonoBehaviour
    {
        private int _myLayer;

        private void Start()
        {
            _myLayer = gameObject.layer;
            Platformer.Core.Logger.Info(Platformer.Core.Logger.LogCategory.General, $"LayerComplianceValidator ACTIVE on layer {_myLayer} ({LayerMask.LayerToName(_myLayer)}).", this);
        }

        private void OnTriggerEnter(Collider other)
        {
            int otherLayer = other.gameObject.layer;
            string myLayerName = LayerMask.LayerToName(_myLayer);
            string otherLayerName = LayerMask.LayerToName(otherLayer);

            Platformer.Core.Logger.Debug(Platformer.Core.Logger.LogCategory.General, $"Trigger detected with '{other.name}' on layer {otherLayer} ({otherLayerName}).", this);

            bool canInteract = !Physics.GetIgnoreLayerCollision(_myLayer, otherLayer);

            if (canInteract)
            {
                Platformer.Core.Logger.Info(Platformer.Core.Logger.LogCategory.General, $"VALIDATION SUCCESS: Physics matrix allows interaction between '{myLayerName}' and '{otherLayerName}'. The issue is likely not layer-related.", this);
            }
            else
            {
                Platformer.Core.Logger.Error(Platformer.Core.Logger.LogCategory.General, $"VALIDATION FAILURE: Physics Layer Collision Matrix is PREVENTING interaction between player layer '{myLayerName}' and trigger layer '{otherLayerName}'. Please update Physics settings.", this);
            }
        }
    }
}