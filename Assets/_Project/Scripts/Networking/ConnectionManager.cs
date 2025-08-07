// File: Assets/_Project/Scripts/Networking/ConnectionManager.cs
// This script handles all Photon PUN 2 network connections, room joining,
// and connection callbacks. It adheres to the project's namespace and
// logging standards.

using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

// The script is placed in the correct namespace for organization.
namespace Platformer.Networking
{
    public class ConnectionManager : MonoBehaviourPunCallbacks
    {
        private GameManager _gameManager;

        void Awake()
        {
            try
            {
                _gameManager = FindFirstObjectByType<GameManager>();
                if (_gameManager == null)
                {
                    // Using the project's standardized logger for consistency.
                    Platformer.Core.Logger.Error(Platformer.Core.Logger.LogCategory.Networking, "GameManager not found in scene! ConnectionManager cannot spawn player.", this);
                }
            }
            catch (System.Exception ex)
            {
                Platformer.Core.Logger.Exception(Platformer.Core.Logger.LogCategory.Networking, ex, "Exception in Awake", this);
            }
        }

        /// <summary>
        /// Initiates connection to Photon Master Server if not already connected.
        /// </summary>
        public void Connect()
        {
            try
            {
                if (!PhotonNetwork.IsConnected)
                {
                    PhotonNetwork.AutomaticallySyncScene = true;
                    Platformer.Core.Logger.Info(Platformer.Core.Logger.LogCategory.Networking, "Connecting to Photon...");
                    PhotonNetwork.ConnectUsingSettings();
                }
                else
                {
                    Platformer.Core.Logger.Info(Platformer.Core.Logger.LogCategory.Networking, "Already connected to Photon.");
                }
            }
            catch (System.Exception ex)
            {
                Platformer.Core.Logger.Exception(Platformer.Core.Logger.LogCategory.Networking, ex, "Exception in Connect", this);
            }
        }

        public override void OnConnectedToMaster()
        {
            Platformer.Core.Logger.Info(Platformer.Core.Logger.LogCategory.Networking, "Connected to Master Server! Joining a random room...");
            PhotonNetwork.JoinRandomRoom();
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Platformer.Core.Logger.Warning(Platformer.Core.Logger.LogCategory.Networking, $"Failed to join a random room (Code: {returnCode}): {message}. Creating a new room...");
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 4 });
        }

        public override void OnJoinedRoom()
        {
            Platformer.Core.Logger.Info(Platformer.Core.Logger.LogCategory.Networking, $"Successfully joined room: {PhotonNetwork.CurrentRoom.Name}");
            
            if (_gameManager == null)
            {
                Platformer.Core.Logger.Error(Platformer.Core.Logger.LogCategory.Networking, "GameManager reference is null! Cannot spawn networked player.", this);
                return;
            }
            
            _gameManager.SpawnNetworkedPlayer();
        }
    }
}