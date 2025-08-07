// File: Assets/_Project/Scripts/Interactables/GoalZone.cs
// Version: 1.2 (Corrected)
// Fix: Changed ScorePoints method return type from 'void' to 'int' to resolve CS0029.

using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace Platformer
{
    [RequireComponent(typeof(Collider))]
    public class GoalZone : MonoBehaviour
    {
        [Header("Settings")]
        public Team team;
        public int scoreCapacity = 100;
        public float timePerCoin = 0.1f;
        public float healPerSecond = 10f;

        private bool _isBroken = false;
        private Coroutine _healingCoroutine;

        public int ScorePoints(int pointsToScore)
        {
            if (_isBroken) return 0;

            int pointsActuallyScored = Mathf.Min(pointsToScore, scoreCapacity);
            scoreCapacity -= pointsActuallyScored;

            if (scoreCapacity <= 0)
            {
                scoreCapacity = 0;
                BreakGoal();
            }
            
            return pointsActuallyScored;
        }
        
        public void BreakGoal()
        {
            if (_isBroken) return;
            
            if (PhotonNetwork.InRoom)
            {
                GetComponent<PhotonView>().RPC(nameof(Rpc_BreakGoal), RpcTarget.All);
            }
            else
            {
                Rpc_BreakGoal();
            }
        }

        [PunRPC]
        private void Rpc_BreakGoal()
        {
            _isBroken = true;
            Core.Logger.Info(Core.Logger.LogCategory.Scoring, $"Goal Zone {name} has been broken!", this);
        }

        #region Player Healing
        public void StartHealingPlayer(PlayerController player)
        {
            if (player.MyStats.team == this.team)
            {
                _healingCoroutine = StartCoroutine(HealPlayer(player.MyStats));
            }
        }

        public void StopHealingPlayer()
        {
            if (_healingCoroutine != null)
            {
                StopCoroutine(_healingCoroutine);
                _healingCoroutine = null;
            }
        }

        private IEnumerator HealPlayer(CharacterStats statsToHeal)
        {
            while (statsToHeal != null && statsToHeal.currentHealth < statsToHeal.baseStats.HP)
            {
                statsToHeal.Heal(Mathf.RoundToInt(healPerSecond * Time.deltaTime));
                yield return null;
            }
        }
        #endregion

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<PlayerScoring>(out var scoring))
            {
                scoring.OnEnterGoalZone(this);
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<PlayerScoring>(out var scoring))
            {
                scoring.OnExitGoalZone(this);
            }
        }
    }
}