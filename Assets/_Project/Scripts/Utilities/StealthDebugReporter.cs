// File: Assets/_Project/Scripts/Utilities/StealthDebugReporter.cs
// Version: 1.1 (Compliant)
// Fix: Corrected all calls to Logger with the full namespace 'Platformer.Core.Logger' to resolve ambiguity.

using UnityEngine;

namespace Platformer.Utilities
{
    [RequireComponent(typeof(CharacterStats))]
    public class StealthDebugReporter : MonoBehaviour
    {
        private CharacterStats _stats;

        private void Awake()
        {
            _stats = GetComponent<CharacterStats>();
            if (_stats == null)
            {
                Platformer.Core.Logger.Error(Platformer.Core.Logger.LogCategory.General, "StealthDebugReporter requires a CharacterStats component.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_stats == null) return;
            
            _stats.OnGrassStatusChanged += HandleGrassStatusChanged;
            _stats.OnRevealStatusChanged += HandleRevealStatusChanged;
            Platformer.Core.Logger.Info(Platformer.Core.Logger.LogCategory.AI, "StealthDebugReporter attached and listening.", this);
        }

        private void OnDisable()
        {
            if (_stats == null) return;

            _stats.OnGrassStatusChanged -= HandleGrassStatusChanged;
            _stats.OnRevealStatusChanged -= HandleRevealStatusChanged;
        }

        private void HandleGrassStatusChanged(bool isInGrass)
        {
            Platformer.Core.Logger.Debug(Platformer.Core.Logger.LogCategory.AI, $"EVENT: OnGrassStatusChanged | New Status: {(isInGrass ? "ENTERED GRASS" : "EXITED GRASS")}", this);
            LogCurrentState();
        }

        private void HandleRevealStatusChanged(bool isRevealed)
        {
            Platformer.Core.Logger.Debug(Platformer.Core.Logger.LogCategory.AI, $"EVENT: OnRevealStatusChanged | New Status: {(isRevealed ? "REVEALED" : "HIDDEN")}", this);
            LogCurrentState();
        }

        [ContextMenu("Log Current Stealth State")]
        private void LogCurrentState()
        {
            bool isVisuallyHidden = _stats.isInGrass && !_stats.isRevealed;
            Platformer.Core.Logger.Info(Platformer.Core.Logger.LogCategory.AI, $"STATE: isInGrass={_stats.isInGrass}, isRevealed={_stats.isRevealed} | RESULT: Should be visible? {!isVisuallyHidden}", this);
        }
    }
}