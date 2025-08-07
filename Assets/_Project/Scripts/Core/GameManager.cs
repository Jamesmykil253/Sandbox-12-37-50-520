// File: Assets/_Project/Scripts/Core/GameManager.cs
// Fix: Added the 'Platformer.Networking' using directive to resolve the compile error.
// Refactor: Moved the class into the 'Platformer' namespace and switched to the
// standardized logger to align with project architecture.
// Refactor: Removed the obsolete AddScore method.

using UnityEngine;
using Photon.Pun;
using Platformer.Networking; // THE FIX IS HERE

namespace Platformer
{
    /// <summary>
    /// Manages game state, player spawning, and online/offline mode transitions.
    /// Singleton pattern ensures only one instance exists.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Tooltip("The player prefab to spawn. Must be in a 'Resources' folder.")]
        [SerializeField]
        private GameObject playerPrefab;
        [Tooltip("An empty GameObject in the scene that marks the player's spawn location.")]
        [SerializeField]
        private Transform spawnPoint;
        private GameObject playerInstance;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        void Start()
        {
            if (PhotonNetwork.OfflineMode)
            {
                SpawnInitialPlayer();
            }
        }

        private void SpawnInitialPlayer()
        {
            try
            {
                if (playerPrefab == null)
                {
                    Core.Logger.Critical(Core.Logger.LogCategory.General, "Player Prefab is not set! Cannot spawn player.", this);
                    return;
                }

                if (spawnPoint == null)
                {
                    Core.Logger.Critical(Core.Logger.LogCategory.General, "Spawn Point is not set! Cannot spawn player.", this);
                    return;
                }

                playerInstance = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
                
                if (playerInstance == null)
                {
                    Core.Logger.Error(Core.Logger.LogCategory.General, "Failed to instantiate player prefab!", this);
                    return;
                }
                
                Core.Logger.Info(Core.Logger.LogCategory.General, $"Initial offline player spawned successfully at {spawnPoint.position}");
            }
            catch (System.Exception ex)
            {
                Core.Logger.Exception(Core.Logger.LogCategory.General, ex, "Exception in SpawnInitialPlayer", this);
            }
        }

        /// <summary>
        /// Transitions from offline to online multiplayer mode.
        /// </summary>
        public void GoOnline()
        {
            try
            {
                if (playerInstance != null)
                {
                    PhotonNetwork.Destroy(playerInstance);
                    playerInstance = null;
                }
                
                PhotonNetwork.OfflineMode = false;
                
                ConnectionManager connectionManager = FindFirstObjectByType<ConnectionManager>();
                if (connectionManager == null)
                {
                    Core.Logger.Error(Core.Logger.LogCategory.Networking, "ConnectionManager not found in scene! Cannot go online.", this);
                    return;
                }
                
                connectionManager.Connect();
                Core.Logger.Info(Core.Logger.LogCategory.Networking, "Transitioning to online mode...");
            }
            catch (System.Exception ex)
            {
                Core.Logger.Exception(Core.Logger.LogCategory.Networking, ex, "Exception in GoOnline", this);
            }
        }

        /// <summary>
        /// Spawns a networked player instance when joining a multiplayer room.
        /// </summary>
        public void SpawnNetworkedPlayer()
        {
            try
            {
                if (playerPrefab == null)
                {
                    Core.Logger.Critical(Core.Logger.LogCategory.General, "Player Prefab is not set! Cannot spawn networked player.", this);
                    return;
                }

                if (spawnPoint == null)
                {
                    Core.Logger.Critical(Core.Logger.LogCategory.General, "Spawn Point is not set! Cannot spawn networked player.", this);
                    return;
                }

                if (!PhotonNetwork.InRoom)
                {
                    Core.Logger.Error(Core.Logger.LogCategory.Networking, "Not in a Photon room! Cannot spawn networked player.", this);
                    return;
                }

                playerInstance = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
                
                if (playerInstance == null)
                {
                    Core.Logger.Error(Core.Logger.LogCategory.General, "Failed to instantiate networked player prefab!", this);
                    return;
                }
                
                Core.Logger.Info(Core.Logger.LogCategory.Networking, $"Networked player spawned successfully for {PhotonNetwork.LocalPlayer.NickName}");
            }
            catch (System.Exception ex)
            {
                Core.Logger.Exception(Core.Logger.LogCategory.General, ex, "Exception in SpawnNetworkedPlayer", this);
            }
        }
    }
}
