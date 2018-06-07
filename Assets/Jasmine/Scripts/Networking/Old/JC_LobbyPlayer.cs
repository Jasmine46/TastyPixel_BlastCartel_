using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class JC_LobbyPlayer : NetworkLobbyPlayer
{
    // Variables & stuff:
    static Color[] mAR_CL_Colours = new Color[] { Color.magenta, Color.red, Color.cyan, Color.blue, Color.yellow };
    // Store already selected colours.
    static List<int> mLS_UsedColours = new List<int>();

    // Buttons in InAddPlayer:
    [SerializeField]
    Button mBT_ColourButton, mBT_ReadyButton, mBT_WaitingPlayerButton, mBT_RemovePlayerButton;

    [SerializeField]
    InputField mBT_NameInput;

    [SerializeField]
    GameObject mGO_LocalIcon, mGO_RemoteIcon;

    // Underlay Colour Lobby Players selection:
    public Color mCL_EvenRowColour = Color.white;
    public Color mCL_OddRowColour = Color.gray;

    [SyncVar(hook = "OnMyName")]
    public string mST_PlayerName = "";

    [SyncVar(hook = "OnMyColour")]
    public Color mCL_PlayerColour = Color.white;

    static Color mCL_JoinColour = new Color(1, 0, 0.4f, 1);
    static Color mCL_NotReadyColour = new Color(0.01f, 0.17f, 0.21f, 1.0f);
    static Color mCL_ReadyColour = new Color(0, 0.8f, 0.8f, 1.0f);
    static Color mCL_Transparent = new Color(0, 0, 0, 0);

    // Methods & stuff:
    // When Create Button is pressed.
    public override void OnClientEnterLobby()
    {
        base.OnClientEnterLobby();

        if (JC_LobbyManager._LobbyMSingleton != null)
        {
            // Add only one Character to the list.
            JC_LobbyManager._LobbyMSingleton.OnPlayerNumberModified(1);
        }

        // Add THIS player to the list of LobbyPlayers (singleton).
        JC_LobbyPlayerList._LobbyPLSingleton.AddPlayer(this);

        if (!isLocalPlayer)
        {
            SetUpLocalPlayer();
        }

        else
        {
            SetUpOtherPlayer();
        }
    }

    private void SetUpLocalPlayer()
    {
        // Get Name of player and activate LobbyPlayer Button:
        mBT_NameInput.interactable = true;
        mGO_RemoteIcon.SetActive(false);
        mGO_LocalIcon.SetActive(true);

        CheckRemoveButton();
        ChangeReadyButtonColour(mCL_JoinColour);

        // Manage player colours:
        if (mCL_PlayerColour == Color.white)
        {
            Cmd_ChangeColour();
        }

        if (mST_PlayerName == "")
        {
            Cmd_ChangeName("Player " + (JC_LobbyPlayerList._LobbyPLSingleton.mRT_PlayerList.childCount - 1));
        }

        mBT_ColourButton.interactable = true;
        mBT_NameInput.interactable = true;
        mBT_RemovePlayerButton.interactable = true;

        mBT_NameInput.onEndEdit.RemoveAllListeners();
        mBT_NameInput.onEndEdit.AddListener(OnNameChanged);

        mBT_ColourButton.onClick.RemoveAllListeners();
        mBT_ColourButton.onClick.AddListener(OnColourChanged);

        mBT_ReadyButton.onClick.RemoveAllListeners();
        mBT_ReadyButton.onClick.AddListener(OnReadyClicked); // Connecting the actual players to the game.

        if (JC_LobbyManager._LobbyMSingleton != null)
        {
            JC_LobbyManager._LobbyMSingleton.OnPlayerNumberModified(0);
        }

        // When reached max number of players remove add Add Player Button.
    }

    public void ToggleJoinButton(bool vReady)
    {
        // If we don't have enough players.
        mBT_ReadyButton.gameObject.SetActive(vReady);
        mBT_WaitingPlayerButton.gameObject.SetActive(!vReady);
    }

    #region LocalPlayerFunctions

    private void ChangeReadyButtonColour(Color vColour)
    {
        ColorBlock tColorBlock = mBT_ReadyButton.colors;

        tColorBlock.normalColor = vColour;
        tColorBlock.pressedColor = vColour;
        tColorBlock.highlightedColor = vColour;
        tColorBlock.disabledColor = vColour;

        mBT_ReadyButton.colors = tColorBlock;
    }

    [ClientRpc]
    public void Rpc_UpdateRemoveButton()
    {
        CheckRemoveButton();
    }

    [ClientRpc]
    public void Rpc_UpdateCountdown(float vCountdown)
    {
        JC_LobbyManager._LobbyMSingleton.mLC_CountdownPanel.vTX_Countdown.text = "Match Starting in " + vCountdown;
        JC_LobbyManager._LobbyMSingleton.mLC_CountdownPanel.gameObject.SetActive(vCountdown != 0);
    }

    private void CheckRemoveButton()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        int tIN_localPlayerCount = 0;

        // Go through all the PlayerControllers (All the connected players in the lobby).
        foreach (PlayerController PC in ClientScene.localPlayers) // All PC controllers in the localPlayers list
        {
            // If playerControllerID == -1, there was either an issue getting information or there is no information being send at all, and it's NOT a local player.
            tIN_localPlayerCount += (PC == null || PC.playerControllerId == -1) ? 0 : 1; // ( ? x : y) Other way of writing if/else.
        }

        mBT_RemovePlayerButton.interactable = tIN_localPlayerCount > 1;
    }

    [Command]
    // Get the colour from the LobbyPlayer panel Image: (!!Assign Textures from the Array!!)
    private void Cmd_ChangeColour()
    {
        int tIndex = Array.IndexOf(mAR_CL_Colours, mCL_PlayerColour);

        int inUseInd = mLS_UsedColours.IndexOf(tIndex);

        if (tIndex < 0)
        {
            tIndex = 0;
        }

        tIndex = (tIndex + 1) % mAR_CL_Colours.Length;

        bool IsAlreadyUsed = false;

        do
        {
            IsAlreadyUsed = false;

            for (int i = 0; i < mLS_UsedColours.Count; i++)
            {
                if (mLS_UsedColours[i] == tIndex)
                {
                    IsAlreadyUsed = true;
                    tIndex = (tIndex + 1) % mAR_CL_Colours.Length;
                }
            }
        }
        while (IsAlreadyUsed);

        if (inUseInd >= 0)
        {
            mLS_UsedColours[inUseInd] = tIndex;
        }

        else
        {
            mLS_UsedColours.Add(tIndex);
        }

        mCL_PlayerColour = mAR_CL_Colours[tIndex];
    }

    [Command]
    // Get the anem from the LobbyPlayer panel Name: 
    private void Cmd_ChangeName(string vName)
    {
        mST_PlayerName = vName;
    }

    // Check name changed in the Input Field:
    private void OnNameChanged(string vText)
    {
        Cmd_ChangeName(vText);
    }

    private void OnColourChanged()
    {
        Cmd_ChangeColour();
    }

    private void OnReadyClicked()
    {
        // PLAYERS READY TO BEGIN!
        SendReadyToBeginMessage();
    }

    public void OnMyName(string vName)
    {
        mST_PlayerName = vName;
        mBT_NameInput.text = mST_PlayerName;
    }

    public void OnMyColour(Color vColour)
    {
        mCL_PlayerColour = vColour;
        mBT_ColourButton.GetComponent<Image>().color = vColour; 
    }

    //20:15

    #endregion

    #region OtherPlayerFunctions
    private void SetUpOtherPlayer()
    {
        mBT_NameInput.interactable = false;
        mBT_RemovePlayerButton.interactable = NetworkServer.active;

        ChangeReadyButtonColour(mCL_NotReadyColour);

        mBT_ReadyButton.transform.GetChild(0).GetComponent<Text>().text = "...";
        mBT_ReadyButton.interactable = false;

        OnClientReady(false);
    }

    public override void OnClientReady(bool readyState)
    {
        if (readyState)
        {
            ChangeReadyButtonColour(mCL_Transparent);
            Text tText = mBT_ReadyButton.transform.GetChild(0).GetComponent<Text>();
            tText.text = "Ready";
            tText.color = mCL_ReadyColour;

            mBT_ReadyButton.interactable = false;
            mBT_ColourButton.interactable = false;
            mBT_NameInput.interactable = false;
        }

        else
        {
            ChangeReadyButtonColour(isLocalPlayer ? mCL_JoinColour : mCL_NotReadyColour);
            Text tText = mBT_ReadyButton.transform.GetChild(0).GetComponent<Text>();
            tText.text = (isLocalPlayer ? "JOIN..." : "...");
            tText.color = Color.white;
            mBT_ReadyButton.interactable = isLocalPlayer;
            mBT_ColourButton.interactable = isLocalPlayer;
            mBT_NameInput.interactable = isLocalPlayer;
        }
    }
    #endregion

    public void OnRemovePlayerClick()
    {
        if (isLocalPlayer)
        {
            RemovePlayer();
        }

        else if(isServer)
        {
            // Could be done directly on ClientDisconnect.
            JC_LobbyManager._LobbyMSingleton.KickPlayer(connectionToClient);
        }
    }

    // This is invoked on behaviours that have authority and NetworkIdendity.localPlayerAuthority.
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        mBT_ReadyButton.transform.GetChild(0).GetComponent<Text>().color = Color.white;
        SetUpLocalPlayer();
    }

    // Just to change the image in the lobby panel;
    public void OnPlayerListChanged(int i)
    {
        GetComponent<Image>().color = (i % 2 == 0) ? mCL_EvenRowColour : mCL_OddRowColour;
    }

    public void OnDestroy()
    {
        JC_LobbyPlayerList._LobbyPLSingleton.RemovePlayerFromList(this);

        if (JC_LobbyManager._LobbyMSingleton != null)
        {
            JC_LobbyManager._LobbyMSingleton.OnPlayerNumberModified(-1);
        }

        int tIndex = Array.IndexOf(mAR_CL_Colours, mCL_PlayerColour);

        if (tIndex < 0)
        {
            return;
        }

        for (int i = 0; i < mLS_UsedColours.Count; i++)
        {
            if (mLS_UsedColours[i] == tIndex)
            {
                mLS_UsedColours.RemoveAt(i);
                break;
            }
        }
    }
}
