using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JC_LobbyMainMenu : MonoBehaviour
{
    JC_LobbyManager mLM_LobbyManager;

    [SerializeField]
    private InputField mIF_MatchNameInput;

    [SerializeField]
    private RectTransform mRT_LobbyServerList;

    // Use this for initialization
    private void OnEnable()
    {
        mLM_LobbyManager = FindObjectOfType<JC_LobbyManager>();
    }

    // Update is called once per frame
    public void OnClickHost()
    {
        mLM_LobbyManager.StartHost();
    }

    public void OnClickJoin()
    {
        mLM_LobbyManager.ChangeToScene(mLM_LobbyManager.mRT_LobbyPanel);
        mLM_LobbyManager.StartClient();
    }

    // Start sending/receiving data from other players.
    // When Create Button is pressed.
    public void OnClickCreateMatchMatchMaking()
    {
        mLM_LobbyManager.StartMatchMaker();
        mLM_LobbyManager.matchMaker.CreateMatch(mIF_MatchNameInput.text, (uint)mLM_LobbyManager.maxPlayers, true, "", "", "", 0, 0, mLM_LobbyManager.OnMatchCreate);
    }

    //When Open Server List is Clicked.
    public void OnClickOpenServerList()
    {
        mLM_LobbyManager.StartMatchMaker();
        mLM_LobbyManager.ChangeToScene(mRT_LobbyServerList);
    }

    // Insert String On Listeners.
}
