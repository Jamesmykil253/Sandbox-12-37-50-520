// File: Assets/_Project/Scripts/Characters/PlayerScoring.cs
// This is the correct, standalone version of the PlayerScoring script.
// This script should be attached to your Player GameObject.

using UnityEngine;
using Photon.Pun;

namespace Platformer
{
    [RequireComponent(typeof(CharacterStats))]
    public class PlayerScoring : MonoBehaviour
    {
        public int CoinCount { get; private set; } = 0;
        public GoalZone CurrentGoalZone { get; private set; }

        private CharacterStats _myStats;
        private PlayerController _playerController;

        private void Awake()
        {
            _myStats = GetComponent<CharacterStats>();
            _playerController = GetComponent<PlayerController>();
        }

        public void CollectCoin(Coin coin)
        {
            if (coin == null) return;

            int value = coin.coinValue;
            coin.Collect();

            if (PhotonNetwork.InRoom && GetComponent<PhotonView>().IsMine)
            {
                GetComponent<PhotonView>().RPC(nameof(Rpc_AddCoin), RpcTarget.All, value);
            }
            else if (!PhotonNetwork.InRoom)
            {
                Rpc_AddCoin(value);
            }
        }

        [PunRPC]
        private void Rpc_AddCoin(int value)
        {
            CoinCount += value;
        }

        public bool CanStartScoring()
        {
            return CurrentGoalZone != null && CoinCount > 0 && (CurrentGoalZone.team == Team.Neutral || CurrentGoalZone.team != _myStats.team);
        }

        public bool ScorePoints()
        {
            if (CurrentGoalZone == null || CoinCount <= 0) return false;

            int pointsToScore = CoinCount;
            int pointsActuallyScored = CurrentGoalZone.ScorePoints(pointsToScore);

            if (pointsActuallyScored > 0)
            {
                if (PhotonNetwork.InRoom && GetComponent<PhotonView>().IsMine)
                {
                    GetComponent<PhotonView>().RPC(nameof(Rpc_SpendCoins), RpcTarget.All, pointsActuallyScored);
                }
                else if (!PhotonNetwork.InRoom)
                {
                    Rpc_SpendCoins(pointsActuallyScored);
                }

                if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.AddScore(_myStats.team, pointsActuallyScored);
                }
                else
                {
                    Core.Logger.Error(Core.Logger.LogCategory.Scoring, "GameStateManager.Instance is null! Ensure the GameStateManager component is on an active GameObject in your scene.", this);
                }
                
                GrantScoringXp(pointsActuallyScored);
                return true;
            }
            return false;
        }

        [PunRPC]
        private void Rpc_SpendCoins(int amount)
        {
            CoinCount -= amount;
        }

        private void GrantScoringXp(int pointsScored)
        {
            int xpGained = pointsScored + (pointsScored / 5 * 10);
            _myStats.AddXp(xpGained);
        }

        public void OnEnterGoalZone(GoalZone zone)
        {
            CurrentGoalZone = zone;
            CurrentGoalZone.StartHealingPlayer(_playerController);
        }

        public void OnExitGoalZone(GoalZone zone)
        {
            if (CurrentGoalZone == zone)
            {
                CurrentGoalZone.StopHealingPlayer();
                CurrentGoalZone = null;
            }
        }
    }
}