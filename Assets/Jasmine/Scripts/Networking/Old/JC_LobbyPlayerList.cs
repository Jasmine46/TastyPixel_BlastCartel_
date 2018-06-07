using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Keep track of players currently in game.
public class JC_LobbyPlayerList : MonoBehaviour
{
    protected List<JC_LobbyPlayer> mLS_LP_List = new List<JC_LobbyPlayer>();
    public static JC_LobbyPlayerList _LobbyPLSingleton;
    public RectTransform mRT_PlayerList;
    public Transform mTR_AddRowButton;

    private void OnEnable()
    {
        _LobbyPLSingleton = this;
    }

    // Link this to the maximum amount of players, and add accordigly.
    public void AddPlayer(JC_LobbyPlayer vLobbyPlayer)
    {
        // If the list already contains this player don't do anything.
        if (mLS_LP_List.Contains(vLobbyPlayer))
        {
            return;
        }

        // Add the lobby player to the list of lobby players.
        mLS_LP_List.Add(vLobbyPlayer);
        vLobbyPlayer.transform.SetParent(mRT_PlayerList, true);

        // Just UI.
        mTR_AddRowButton.SetAsLastSibling();
        PlayerListModified();

        // Debug me!!
        foreach (JC_LobbyPlayer PC in mLS_LP_List)
        {
            print("LobbyPlayer: " + PC);
        }
    }

    public void RemovePlayerFromList(JC_LobbyPlayer vPlayer)
    {
        mLS_LP_List.Remove(vPlayer);
        PlayerListModified();
    }

    public void PlayerListModified()
    {
        int i = 0;

        foreach (JC_LobbyPlayer PC in mLS_LP_List)
        {
            PC.OnPlayerListChanged(i);
            i++;
        }
    }
}
