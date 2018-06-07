using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JC_PUNMatchSettings : Photon.PunBehaviour
{
    private byte mBY_MaxAmountOfPlayers;
    private string mST_MatchName;
    // When creating a match also add to the list of matches.
    // Add Team Settings?
    // Add if Match is private or can be joined by anyone. // if friends?

    public void OnEnable()
    {

    }

    // Getters / Setters
    byte GetAmountOfPlayers()
    {
        return mBY_MaxAmountOfPlayers;
    }

    void SetAmountOfPlayer(byte vAmountOfPlayers)
    {
        if (vAmountOfPlayers != mBY_MaxAmountOfPlayers)
        {
            mBY_MaxAmountOfPlayers = vAmountOfPlayers;
        }
    }

    string GetMatchName()
    {
        return mST_MatchName;
    }

    void SetMatchName(string vMatchName)
    {
        if (vMatchName != mST_MatchName)
        {
            mST_MatchName = vMatchName;
        }
    }

}
