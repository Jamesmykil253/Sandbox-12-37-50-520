// File: Assets/_Project/Scripts/Core/StatBlock.cs
// Version: 2.0 (Merged & Architecturally Compliant)
// Status: APPROVED

using UnityEngine;

namespace Platformer
{
    [System.Serializable]
    public struct StatBlock
    {
        [Tooltip("Health Points: The character's total health pool.")]
        public int HP;

        [Tooltip("Physical Attack: Damage dealt by basic attacks.")]
        public int Attack;

        [Tooltip("Physical Defense: Damage reduction from basic attacks.")]
        public int Defense;
        
        [Tooltip("Special Attack: Damage dealt by special abilities.")]
        public int SpecialAttack;

        [Tooltip("Special Defense: Damage reduction from special abilities.")]
        public int SpecialDefense;

        [Tooltip("Movement Speed: How fast the character moves.")]
        public float Speed;

        [Tooltip("Critical Hit Rate: Chance to deal extra damage (0.0 to 1.0).")]
        [Range(0f, 1f)]
        public float CritRate;

        [Tooltip("Attacks Per Second: How many basic attacks the character can perform in one second.")]
        public float AttackSpeed;
    }
}