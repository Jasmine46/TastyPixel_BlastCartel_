using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;

public class JC_LobbyServerEntry : MonoBehaviour
{
    [SerializeField] Text mTX_ServerInfoText;
    [SerializeField] Text mTX_PlayersInRoom;
    [SerializeField] Button mBT_JoinButton;

    public void Populate(MatchInfoSnapshot vMatch, JC_LobbyManager vLobbyManager, Color vColour)
    {
        mTX_ServerInfoText.text = vMatch.name;
        mTX_PlayersInRoom.text = vMatch.currentSize.ToString() + " / " + vMatch.maxSize.ToString();

        mBT_JoinButton.onClick.RemoveAllListeners();
        mBT_JoinButton.onClick.AddListener(() => JoinMatch(vMatch.networkId, vLobbyManager));
    }

    public void JoinMatch(NetworkID vNetworkID, JC_LobbyManager vLobbyManager)
    {
        vLobbyManager.matchMaker.JoinMatch(vNetworkID, "", "", "", 0, 0, vLobbyManager.OnMatchJoined);
    }
}
