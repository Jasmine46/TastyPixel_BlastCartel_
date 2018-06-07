using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class JC_LobbyManager : NetworkLobbyManager
{
    private RectTransform mRT_CurrentPanel;
    public RectTransform mRT_LobbyPanel;
    [SerializeField] RectTransform mRT_MainMenuPanel;

    public static JC_LobbyManager _LobbyMSingleton;
    private ulong mUL_CurrentMatchID;
    private int mIN_PlayerNumber;

    public float mFL_CountdownTime;
    [SerializeField] public JC_LobbyCountdownPanel mLC_CountdownPanel;

    bool mBL_DisconnectServer;

    NetworkConnection mNC_NetworkConnection;

    private void OnEnable()
    {
        _LobbyMSingleton = this;
        DontDestroyOnLoad(gameObject);

        mRT_CurrentPanel = mRT_MainMenuPanel;
        //print("LobbyManager: " + mRT_CurrentPanel.name);
    }

    // Hook invoked when starting the host.
    public override void OnStartHost()
    {
        base.OnStartHost();

        // Switch to Lobby Panel
        ChangeToScene(mRT_LobbyPanel);

        print("OnStartHost()");
    }

    public void ChangeToScene(RectTransform vPanel) 
    {
        if (mRT_CurrentPanel != null)
        {
            mRT_CurrentPanel.gameObject.SetActive(false);
        }

        if (vPanel != null && vPanel.gameObject.activeInHierarchy == false)
        {
            vPanel.gameObject.SetActive(true);
        }

        mRT_CurrentPanel = vPanel;
    }

    public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        base.OnMatchCreate(success, extendedInfo, matchInfo);

        mUL_CurrentMatchID = (ulong)matchInfo.networkId;
    }

    public override void OnDestroyMatch(bool success, string extendedInfo)
    {
        base.OnDestroyMatch(success, extendedInfo);

        if (mBL_DisconnectServer)
        {
            StopMatchMaker();
            StopHost();
        }
    }

    public override void OnLobbyClientSceneChanged(NetworkConnection conn)
    {
        //base.OnLobbyClientSceneChanged(conn);

        if (networkSceneName == playScene.ToString())
        {
            mRT_MainMenuPanel.gameObject.SetActive(false);
            mRT_LobbyPanel.gameObject.SetActive(false);
        }
    }

    public override void OnLobbyServerSceneChanged(string sceneName)
    {
        if (sceneName == playScene.ToString())
        {
            mRT_MainMenuPanel.gameObject.SetActive(false);
            mRT_LobbyPanel.gameObject.SetActive(false);
        }
    }

    public override GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId)
    {
        GameObject vObj = Instantiate(lobbyPlayerPrefab.gameObject) as GameObject;
        JC_LobbyPlayer vNewPlayer = vObj.GetComponent<JC_LobbyPlayer>();

        vNewPlayer.ToggleJoinButton(numPlayers + 1 >= minPlayers);

        // Amount of available lobby slots (remaining available connections).
        for (int i = 0; i < lobbySlots.Length; i++)
        {
            JC_LobbyPlayer vPC = lobbySlots[i] as JC_LobbyPlayer;

            if (vPC != null)
            {
                vPC.Rpc_UpdateRemoveButton();
                vPC.ToggleJoinButton(numPlayers + 1 >= minPlayers);
            }
        }

        return vObj;
    }

    public override void OnLobbyServerPlayerRemoved(NetworkConnection conn, short playerControllerId)
    {
        for (int i = 0; i < lobbySlots.Length; i++)
        {
            JC_LobbyPlayer vPC = lobbySlots[i] as JC_LobbyPlayer;

            if (vPC != null)
            {
                vPC.Rpc_UpdateRemoveButton();
                vPC.ToggleJoinButton(numPlayers + 1 >= minPlayers);
            }
        }
    }

    public override void OnLobbyServerDisconnect(NetworkConnection conn)
    {
        for (int i = 0; i < lobbySlots.Length; i++)
        {
            JC_LobbyPlayer vPC = lobbySlots[i] as JC_LobbyPlayer;

            if (vPC != null)
            {
                vPC.Rpc_UpdateRemoveButton();
                vPC.ToggleJoinButton(numPlayers + 1 >= minPlayers);
            }
        }
    }

    public override void OnLobbyServerPlayersReady()
    {
        bool allPlayersReady = true;

        for (int i = 0; i < lobbySlots.Length; i++)
        {
            if (lobbySlots[i] != null)
            {
                allPlayersReady &= lobbySlots[i].readyToBegin;
            }
        }

        if (allPlayersReady)
        {
            StartCoroutine(ServerCountdown());
        }
    }

    public IEnumerator ServerCountdown()
    {
        float vRemainingTime = mFL_CountdownTime;
        int vFloortime = Mathf.FloorToInt(vRemainingTime);

        while (vRemainingTime > 0)
        {
            yield return null;

            vRemainingTime -= Time.deltaTime;
            int newFloorTime = Mathf.FloorToInt(vRemainingTime);

            if (newFloorTime != vFloortime)
            {
                vFloortime = newFloorTime;

                for (int i = 0; i < lobbySlots.Length; i++)
                {
                    if (lobbySlots[i] != null)
                    {
                        (lobbySlots[i] as JC_LobbyPlayer).Rpc_UpdateCountdown(vFloortime);
                    }
                }
            }
        }

        for (int i = 0; i < lobbySlots.Length; i++)
        {
            if (lobbySlots[i] != null)
            {
                (lobbySlots[i] as JC_LobbyPlayer).Rpc_UpdateCountdown(0);
            }
        }

        ServerChangeScene(playScene);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        //This registers a handler function for a message Id.
        //conn.RegisterHandler(MsgKicked, Kicked)

        conn.RegisterHandler(MsgKicked, KickedMessage);

        if (!NetworkServer.active)
        {
            ChangeToScene(mRT_LobbyPanel);
        }
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        ChangeToScene(mRT_MainMenuPanel);
    }

    public override void OnClientError(NetworkConnection conn, int errorCode)
    {
        ChangeToScene(mRT_MainMenuPanel);
        // Show Info Display Here: why did we get an error?
    }

    // When a player is removed from the game, it recalculated the amount of players.
    // Allow to add/remove player.
    public void OnPlayerNumberModified(int vCount)
    {
        mIN_PlayerNumber += vCount;

        int tIN_localPlayerCount = 0;

        // Go through all the PlayerControllers (All the connected players in the lobby).
        foreach (PlayerController PC in ClientScene.localPlayers) // All PC controllers in the localPlayers list
        {
            // If playerControllerID == -1, there was either an issue getting information or there is no information being send at all, and it's NOT a local player.
            tIN_localPlayerCount += (PC == null || PC.playerControllerId == -1) ? 0 : 1; // ( ? x : y) Other way of writing if/else.
        }
    }

    class KickMessage : MessageBase { }

    public short MsgKicked = MsgType.Highest + 1;

    public void KickPlayer(NetworkConnection vConn)
    {
        vConn.Send(MsgKicked, new KickMessage());
    }

    public void KickedMessage(NetworkMessage vNetMsg)
    {
        // Shows kicked user.
        vNetMsg.conn.Disconnect();
    }

    // Server Management:
    public void AddLocalPlayer()
    {
        TryToAddPlayer();
    }

    public void RemovePlayer(JC_LobbyPlayer vPlayer)
    {
        vPlayer.RemovePlayer();
    }
}
