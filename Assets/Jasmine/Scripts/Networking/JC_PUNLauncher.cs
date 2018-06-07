using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.CompanyName.Minions
{
    public class JC_PUNLauncher : Photon.PunBehaviour
    {
        [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
        public byte MaxPlayersPerRoom = 4;

        public PhotonLogLevel photonLog = PhotonLogLevel.Informational;

        //https://doc.photonengine.com/en-us/pun/current/demos-and-tutorials/pun-basics-tutorial/lobby
        // Client's version number, each client has a different one. (similar to NetID?)
        string mST_GameVersion = "1"; // Guessing it also stored the setting of the connection?

        void Awake()
        {
            // No need to join the lobby to get the list of rooms available.
            PhotonNetwork.autoJoinLobby = false;

            // Makes sure that PhotonNetwork.LoadLevel() is invokable on master and other clients.
            PhotonNetwork.automaticallySyncScene = true;

            PhotonNetwork.logLevel = photonLog;
        }

        // If already connected attept joining a random room.
        void Start()
        {
            Connect();
        }

        public void Connect()
        {
            // If we are connected to the server join a random room.
            if (PhotonNetwork.connected)
            {
                // Attempts to connect to a match.
                PhotonNetwork.JoinRandomRoom();

                print("JC_PUNLauncher/ Connect()/ PhotonNetwork.JoinRandomRoom()");
            }

            else
            {
                PhotonNetwork.ConnectUsingSettings(mST_GameVersion);

                print("JC_PUNLauncher/ Connect()/ PhotonNetwork.ConnectUsingSettings(mST_GameVersion)");
            }
        }

        public override void OnConnectedToPhoton()
        {
            base.OnConnectedToPhoton();

            Debug.Log("Test/Launcher: OnConnectedToPhoton(), called by PUN");
        }

        public override void OnConnectedToMaster()
        {
            PhotonNetwork.JoinRandomRoom();

            Debug.Log("Test/Launcher: OnConnectedToMaster(), called by PUN");
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Test/Launcher: OnJoinedRoom(), called by PUN. Now this client is in a room.");
        }

        public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
        {
            Debug.Log("Test/Launcher:OnPhotonRandomJoinFailed(), called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 4}, null);");

            //PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 4 }, null);
            // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
            PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = MaxPlayersPerRoom }, null);
        }

        public override void OnDisconnectedFromPhoton()
        {
            base.OnDisconnectedFromPhoton();

            Debug.LogWarning("Test/Launcher: OnDisconnectedFromPhoton(), called by PUN");
        }
    }
}