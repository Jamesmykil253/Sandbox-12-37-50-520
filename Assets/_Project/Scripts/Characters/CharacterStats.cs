// File: Assets/_Project/Scripts/Characters/CharacterStats.cs
// Version: 2.2 (Augmented)
// Fix: Added missing methods and events (Heal, OnLevelUp, RegisterAttack) to resolve CS1061 errors.
// Fix: Implemented invocation for OnRevealStatusChanged to resolve CS0067 warning.

using UnityEngine;
using Photon.Pun;
using System;
using System.Collections;

namespace Platformer
{
    [RequireComponent(typeof(PhotonView))]
    public class CharacterStats : MonoBehaviourPunCallbacks, IPunObservable
    {
        [Header("Configuration")]
        [SerializeField] private StatBlock _baseStats;
        public StatBlock baseStats => _baseStats;
        
        [SerializeField] private Team _team;
        public Team team => _team;

        [Header("State")]
        public int level { get; private set; } = 1;
        public int currentHealth { get; private set; }
        public int currentXp { get; private set; }
        public bool isInGrass { get; private set; }
        public bool isRevealed { get; private set; }
        private bool _isDead = false;

        [Header("Empowered Attack")]
        private const int ATTACKS_UNTIL_EMPOWERED = 3;
        private int _basicAttackCounter = 0;

        // --- EVENTS ---
        public event Action OnDied;
        public event Action OnDamageTaken;
        public event Action<bool> OnGrassStatusChanged;
        public event Action<bool> OnRevealStatusChanged;
        public event Action<int> OnLevelUp;

        private Coroutine _revealCoroutine;

        private void Awake()
        {
            currentHealth = _baseStats.HP;
        }
        
        public void AddXp(int amount)
        {
            if (_isDead) return;
            currentXp += amount;
            
            int xpForNextLevel = 100 + (level * 50); // Example XP curve
            if (currentXp >= xpForNextLevel)
            {
                level++;
                currentXp -= xpForNextLevel;
                currentHealth = baseStats.HP; // Full heal on level up
                OnLevelUp?.Invoke(level);
                Core.Logger.Info(Core.Logger.LogCategory.Combat, $"{name} leveled up to {level}!", this);
            }
        }

        public void Heal(int amount)
        {
            if (photonView.IsMine)
            {
                photonView.RPC(nameof(Rpc_Heal), RpcTarget.All, amount);
            }
        }

        [PunRPC]
        private void Rpc_Heal(int amount)
        {
            if (_isDead) return;
            currentHealth = Mathf.Min(currentHealth + amount, baseStats.HP);
            Core.Logger.Info(Core.Logger.LogCategory.Combat, $"{name} healed for {amount}. Current HP: {currentHealth}", this);
        }

        [PunRPC]
        public void Rpc_TakeDamage(int damage, int attackerViewID)
        {
            if (_isDead) return;
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, _baseStats.HP);
            OnDamageTaken?.Invoke();
            if (currentHealth <= 0)
            {
                Die(attackerViewID);
            }
        }
        
        private void Die(int killerViewID)
        {
            _isDead = true;
            OnDied?.Invoke();
            Core.Logger.Info(Core.Logger.LogCategory.Combat, $"{name} was defeated.", this);
        }

        #region Empowered Attack Logic
        public bool IsNextAttackEmpowered()
        {
            return _basicAttackCounter >= ATTACKS_UNTIL_EMPOWERED - 1;
        }

        public void ConsumeEmpoweredAttack()
        {
            _basicAttackCounter = 0;
        }

        // Renamed from RegisterPlayerHit to match codebase dependencies
        public void RegisterAttack()
        {
            if (_basicAttackCounter < ATTACKS_UNTIL_EMPOWERED - 1)
            {
                _basicAttackCounter++;
            }
        }
        #endregion

        #region Stealth Logic
        public void SetInGrassStatus(bool inGrass)
        {
            if (isInGrass == inGrass) return; 
            isInGrass = inGrass;
            Core.Logger.Debug(Core.Logger.LogCategory.AI, $"Character '{name}' InGrass status set to: {isInGrass}", this);
            OnGrassStatusChanged?.Invoke(isInGrass);
        }

        public void RevealCharacter(float duration)
        {
            if (_revealCoroutine != null)
            {
                StopCoroutine(_revealCoroutine);
            }
            _revealCoroutine = StartCoroutine(RevealTimer(duration));
        }

        private IEnumerator RevealTimer(float duration)
        {
            isRevealed = true;
            OnRevealStatusChanged?.Invoke(true); // Fixes the unused event warning
            Core.Logger.Debug(Core.Logger.LogCategory.AI, $"Character '{name}' has been revealed for {duration}s.", this);
            yield return new WaitForSeconds(duration);
            isRevealed = false;
            OnRevealStatusChanged?.Invoke(false);
            Core.Logger.Debug(Core.Logger.LogCategory.AI, $"Character '{name}' is no longer revealed.", this);
            _revealCoroutine = null;
        }
        #endregion

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(currentHealth);
                stream.SendNext(level);
                stream.SendNext(_basicAttackCounter);
            }
            else
            {
                this.currentHealth = (int)stream.ReceiveNext();
                this.level = (int)stream.ReceiveNext();
                this._basicAttackCounter = (int)stream.ReceiveNext();
            }
        }
    }
}