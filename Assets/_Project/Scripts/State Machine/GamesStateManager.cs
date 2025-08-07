// File: Assets/_Project/Scripts/Core/GameStateManager.cs
// Version: 2.0 (Architecturally Compliant)
// Status: APPROVED

using UnityEngine;
using Photon.Pun;
using System;

namespace Platformer
{
    [RequireComponent(typeof(PhotonView))]
    public class GameStateManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        private static GameStateManager _instance;
        private const string PREFAB_PATH = "Systems/GameStateManager";

        public static GameStateManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var prefab = Resources.Load<GameStateManager>(PREFAB_PATH);
                    if (prefab == null)
                    {
                        Core.Logger.Critical(Core.Logger.LogCategory.Networking, $"CRITICAL: GameStateManager prefab not found at path: Resources/{PREFAB_PATH}");
                        return null;
                    }
                    // Instantiate with a null parent to guarantee it's a root object, resolving the DontDestroyOnLoad warning.
                    _instance = Instantiate(prefab, null);
                }
                return _instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticInstance()
        {
            _instance = null;
        }

        public enum GameState { Pregame, Running, FinalStretch, Postgame }

        [Header("Game State")]
        public GameState CurrentState { get; private set; }
        public float MatchTimeRemaining { get; private set; } = 300f;

        [Header("Team Scores")]
        public int HomeTeamScore { get; private set; }
        public int AwayTeamScore { get; private set; }

        public static event Action<GameState> OnGameStateChanged;
        public static event Action<float> OnTimerUpdated;
        public static event Action<int, int> OnScoreUpdated;

        private const float FINAL_STRETCH_TIME = 60f;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        public override void OnJoinedRoom()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SetGameState(GameState.Pregame);
                Invoke(nameof(StartGame), 5f);
            }
        }

        private void StartGame()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            SetGameState(GameState.Running);
        }

        private void Update()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (CurrentState != GameState.Running && CurrentState != GameState.FinalStretch) return;

            MatchTimeRemaining -= Time.deltaTime;

            if (CurrentState == GameState.Running && MatchTimeRemaining <= FINAL_STRETCH_TIME)
            {
                SetGameState(GameState.FinalStretch);
            }

            if (MatchTimeRemaining <= 0)
            {
                MatchTimeRemaining = 0;
                SetGameState(GameState.Postgame);
            }
        }

        public void AddScore(Platformer.Team team, int points)
        {
            if (PhotonNetwork.InRoom)
            {
                photonView.RPC(nameof(Rpc_UpdateScore), RpcTarget.All, team, points);
            }
            else
            {
                Rpc_UpdateScore(team, points);
            }
        }

        [PunRPC]
        private void Rpc_UpdateScore(Platformer.Team team, int points)
        {
            if (team == Platformer.Team.Home) HomeTeamScore += points;
            else if (team == Platformer.Team.Away) AwayTeamScore += points;
            OnScoreUpdated?.Invoke(HomeTeamScore, AwayTeamScore);
        }

        private void SetGameState(GameState newState)
        {
            if (CurrentState == newState) return;

            if (PhotonNetwork.InRoom)
            {
                photonView.RPC(nameof(Rpc_SetGameState), RpcTarget.All, newState);
            }
            else
            {
                Rpc_SetGameState(newState);
            }
        }

        [PunRPC]
        private void Rpc_SetGameState(GameState newState)
        {
            CurrentState = newState;
            OnGameStateChanged?.Invoke(CurrentState);
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(MatchTimeRemaining);
            }
            else
            {
                this.MatchTimeRemaining = (float)stream.ReceiveNext();
                OnTimerUpdated?.Invoke(this.MatchTimeRemaining);
            }
        }
    }
}