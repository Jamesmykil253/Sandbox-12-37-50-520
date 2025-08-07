// File: Assets/_Project/Scripts/Interactables/StealthGrass.cs
// Version: 1.1 (Syntactically Correct)
// Fix: Removed invalid 'public' access modifiers from Unity message functions.

using UnityEngine;

namespace Platformer
{
    [RequireComponent(typeof(Collider))]
    public class StealthGrass : MonoBehaviour
    {
        private void Awake()
        {
            // Ensure the collider is a trigger, as this script will not function otherwise.
            var col = GetComponent<Collider>();
            if (!col.isTrigger)
            {
                Platformer.Core.Logger.Warning(Platformer.Core.Logger.LogCategory.General, $"Collider on {name} is not set to 'Is Trigger'. Stealth system will not work.", this);
            }
        }

        // CORRECT: No access modifier (private by default, which is valid).
        void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<CharacterStats>(out var stats))
            {
                stats.SetInGrassStatus(true);
            }
        }

        // CORRECT: No access modifier.
        void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<CharacterStats>(out var stats))
            {
                stats.SetInGrassStatus(false);
            }
        }
    }
}