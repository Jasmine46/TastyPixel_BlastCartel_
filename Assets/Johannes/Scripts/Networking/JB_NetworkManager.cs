using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public struct ActivePlayer
{
    public ActivePlayer(ControlPC _pc, int _teamNo, int _refID, string _name)
    {
        playerObject = _pc;
        teamNumber = _teamNo;
        referenceID = _refID;
        playerName = _name;
    }

    public ControlPC playerObject;
    public int teamNumber;
    public int referenceID;
    public string playerName;
}

public class JB_NetworkManager : MonoBehaviour
{
    // Player references
    public List<ActivePlayer> allPlayers;
    private Dictionary<int, ControlPC> playerReferenceLite;
    private Dictionary<int, ActivePlayer> playerReferenceFull;

    // Scene variables

    void Start()
    {

    }


    void Update()
    {

    }
    
    public void AllPlayersAdded()
    {
        SetReferencesToPCs();
        ChangeAllyPlayerLayers();
        CountTeams();
    }

    void SetReferencesToPCs()
    {
        // Reference to PC
        foreach (ActivePlayer _pc in allPlayers)    // set up quick access to PC script by reference ID
        {
            playerReferenceLite.Add(_pc.referenceID, _pc.playerObject);
        }
        foreach (ActivePlayer _pc in allPlayers)    // set up access to full PC detail
        {
            playerReferenceFull.Add(_pc.referenceID, _pc);
        }
    }

    void ChangeAllyPlayerLayers()
    {
        // Change players' ally hitboxes
        for (int i = 0; i < allPlayers.Count; i++)
        {
            for (int j = 0; j < allPlayers.Count; j++)
            {
                if (allPlayers[i].teamNumber == allPlayers[j].teamNumber)
                {
                    // if first player is on same team as second player set second player's hitboxes to another layer in first player's client
                    allPlayers[i].playerObject.RpcChangeAllyLayer(allPlayers[j].playerObject.gameObject);
                }
            }
        }
    }

    void CountTeams()
    {
        if (allPlayers.Count > 0)
        {
            int teamCount = 1;
            List<int> allTeams = new List<int>();
            allTeams.Add(allPlayers[0].teamNumber);
            for (int i = 1; i < allPlayers.Count; i++)
            {
                for (int j = 0; j < allTeams.Count; j++)
                {
                    if (allPlayers[i].teamNumber == allTeams[j])
                    {
                        break;
                    }
                    allTeams.Add(allPlayers[i].teamNumber);
                }
            }
            JB_GameManager.gm.numberOfTeams = teamCount;
        }
        else
        {
            Debug.LogError("No players added to Network Manager");
            JB_GameManager.gm.numberOfTeams = 0;
        }
    }
}
