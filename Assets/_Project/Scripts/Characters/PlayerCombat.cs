// File: Assets/_Project/Scripts/Characters/PlayerCombat.cs
// Version: 1.1 (Method call corrected)
// Fix: Updated method call from RegisterPlayerHit to RegisterAttack for consistency with CharacterStats.

using UnityEngine;
using Photon.Pun;

namespace Platformer
{
    [RequireComponent(typeof(CharacterStats))]
    [RequireComponent(typeof(TargetingSystem))]
    [RequireComponent(typeof(PhotonView))]
    [RequireComponent(typeof(Animator))]
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Combat Settings")]
        public float meleeAttackRadius = 2f;
        public GameObject boostedProjectilePrefab;
        public float attackMoveSpeedMultiplier = 0.5f;
        public LayerMask attackLayerMask;
        public float revealDurationOnAttack = 2f;

        public bool IsAttackOnCooldown => _attackCooldownTimer > 0f;
        
        private float _attackCooldownTimer;
        private CharacterStats _myStats;
        private TargetingSystem _targetingSystem;
        private PhotonView _photonView;
        private Animator _animator;

        private void Awake()
        {
            _myStats = GetComponent<CharacterStats>();
            _targetingSystem = GetComponent<TargetingSystem>();
            _photonView = GetComponent<PhotonView>();
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (_attackCooldownTimer > 0)
            {
                _attackCooldownTimer -= Time.deltaTime;
            }
        }

        public void ExecuteAttack()
        {
            if (IsAttackOnCooldown) return;

            _attackCooldownTimer = 1f / _myStats.baseStats.AttackSpeed;
            if (_myStats.isInGrass)
            {
                _myStats.RevealCharacter(revealDurationOnAttack);
            }
            PlayAttackAnimation();

            if (_myStats.IsNextAttackEmpowered())
            {
                FireBoostedProjectile();
                _myStats.ConsumeEmpoweredAttack();
            }
            else
            {
                PerformMeleeAttack();
            }
        }

        private void PerformMeleeAttack()
        {
            if (!_photonView.IsMine) return;

            Collider[] hits = Physics.OverlapSphere(transform.position, meleeAttackRadius, attackLayerMask);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<Hurtbox>(out var hurtbox) && hurtbox.statsController != _myStats)
                {
                    var targetStats = hurtbox.statsController;
                    if (targetStats != null && targetStats.team != _myStats.team)
                    {
                        int damage = CombatCalculator.CalculateDamage(_myStats, targetStats, isEmpowered: false);
                        
                        if (PhotonNetwork.InRoom)
                        {
                            PhotonView targetView = targetStats.GetComponent<PhotonView>();
                            if (targetView != null)
                            {
                                targetView.RPC("Rpc_TakeDamage", RpcTarget.All, damage, _photonView.ViewID);
                            }
                        }
                        else
                        {
                            targetStats.Rpc_TakeDamage(damage, _photonView.ViewID);
                        }

                        if (targetStats.team != Team.Neutral)
                        {
                           _myStats.RegisterAttack(); // CORRECTED
                        }
                        
                        break; 
                    }
                }
            }
        }

        private void FireBoostedProjectile()
        {
            if (boostedProjectilePrefab == null || !_photonView.IsMine) return;

            Transform target = _targetingSystem.FindBestTarget();
            Vector3 spawnPos = transform.position + transform.forward;
            Quaternion projectileRotation = target != null ? 
                Quaternion.LookRotation((target.position - transform.position).normalized) : 
                transform.rotation;
            
            if (PhotonNetwork.InRoom)
            {
                var projectileGO = PhotonNetwork.Instantiate(
                    boostedProjectilePrefab.name, 
                    spawnPos, 
                    projectileRotation
                );
                
                var projectile = projectileGO.GetComponent<Projectile>();
                if (projectile != null)
                {
                    int damage = CombatCalculator.CalculateDamage(_myStats, null, isEmpowered: true);
                    projectile.Initialize(_myStats, damage, true, attackLayerMask);
                }
            }
        }
        
        private void PlayAttackAnimation()
        {
            if (PhotonNetwork.InRoom)
            {
                _photonView.RPC(nameof(Rpc_PlayAttackAnimation), RpcTarget.All);
            }
            else
            {
                Rpc_PlayAttackAnimation();
            }
        }

        [PunRPC]
        private void Rpc_PlayAttackAnimation()
        {
            if (_animator != null && _animator.runtimeAnimatorController != null)
            {
                _animator.SetTrigger("Attack");
            }
        }
    }
}