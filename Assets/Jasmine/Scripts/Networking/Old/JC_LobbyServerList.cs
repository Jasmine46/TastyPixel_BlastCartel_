using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;

public class JC_LobbyServerList : MonoBehaviour
{
    JC_LobbyManager mLM_LobbyManager;
    public GameObject mGO_ServerListItemPrefab;
    [SerializeField] RectTransform mRT_ServerListPanel;

    public Color mCL_EvenRowColour = Color.white;
    public Color mCL_OddRowColour = Color.gray;

    int mIN_CurrentPage = 0;
    int mIN_PreviousPage = 0;

    private void OnEnable()
    {
        if (mLM_LobbyManager == null)
        {
            mLM_LobbyManager = FindObjectOfType<JC_LobbyManager>();
        }

        mIN_CurrentPage = 0;
        mIN_PreviousPage = 0;

        foreach (Transform vTranform in mRT_ServerListPanel)
        {
            Destroy(vTranform.gameObject);
        }

        RequestPage(0);
    }

    public void OnMatchList(bool vSuccess, string vExtendInfo, List<MatchInfoSnapshot> vMatches)
    {
        if (vMatches.Count == 0)
        {
            return;
        }

        for (int i = 0; i < vMatches.Count; i++)
        {
            GameObject vServerItem = Instantiate(mGO_ServerListItemPrefab) as GameObject;

            vServerItem.GetComponent<JC_LobbyServerEntry>().Populate(vMatches[i], mLM_LobbyManager, (i % 2 == 0) ? mCL_OddRowColour : mCL_EvenRowColour);

            vServerItem.transform.SetParent(mRT_ServerListPanel, false);
        }
    }

    public void RequestPage(int vPage)
    {
        mIN_PreviousPage = mIN_CurrentPage;
        mIN_CurrentPage = vPage;

        mLM_LobbyManager.matchMaker.ListMatches(vPage, 6, "", true, 0, 0, OnMatchList);
    }
}
