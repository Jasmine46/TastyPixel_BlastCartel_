using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class JC_MainMenu : MonoBehaviour
{
    [SerializeField] private Button mBT_Play;
    [SerializeField] private Button mBT_Options;
    [SerializeField] private Button mBT_Quit;

    private ScrollRect mSR_PlayPanel;
    private ScrollRect mSR_Options;
    private ScrollRect mSR_Quit;

    // Use this for initialization
    void Start()
    {
        //mSR_PlayPanel.gameObject.SetActive(false);
        //mSR_Options.gameObject.SetActive(false);
        //mSR_Quit.gameObject.SetActive(false);

        mBT_Play.GetComponent<Button>().onClick.AddListener(Play);
        mBT_Options.GetComponent<Button>().onClick.AddListener(Options);
        mBT_Quit.GetComponent<Button>().onClick.AddListener(Quit);
    }

    private void Play()
    {
        SceneManager.LoadScene("PUNLobbyMenu");
        //PhotonNetwork.LoadLevel(1);
        //mSR_PlayPanel.gameObject.SetActive(true);
    }

    private void Options()
    {
        mSR_Options.gameObject.SetActive(true);
    }

    private void Quit()
    {
        Application.Quit();
    }
}
