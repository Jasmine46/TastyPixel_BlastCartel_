using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class JC_PUNManager : Photon.PunBehaviour
{
    [HideInInspector] public static JC_PUNManager instance = null;
    [HideInInspector] public static JB_GameManager GMinstance = null;

    // App ID: 7131a50e-4a71-499f-9ac9-6b77fb712ad2
    private string mST_GameVersion = "0.1v";

    // Serialized
    [Header("UI Elements")]
    [SerializeField] private RectTransform mRT_MainPanel;
    [SerializeField] private Button mBT_CreateMatch;
    [SerializeField] private RectTransform mRT_MatchSettingsPanel;
    [SerializeField] private ScrollRect mSR_CurrentMatches;
    [SerializeField] private RectTransform mRT_MatchPanelElement;
    [SerializeField] private Text mTX_NoMatchAvailable;
    [SerializeField] private Text mTX_AvailableMatches;
    [SerializeField] private InputField mIF_MatchName;

    private string mST_MatchName;
    private string mST_JoinMatchName;

    [Header("Network Settings")]
    // Private
    private Button mBT_JoinOtherMatch; // Take you to Character/Weapon selection.
    private Text mTX_OtherMatchName;
    private Text mTX_PlayersInOtherMatch;

    // Room Creation Options:
    private RoomOptions mRO_RoomOpt; // = new RoomOptions();
    private TypedLobby mTL_TypedLobby; // Set privacy settings for the room.

    //[SerializeField] private int mIN_MaxAmountOfPlayers;

    // Add Team options
    public GameObject mGO_PlayerPrefab;
    private JC_PUNSpawner[] mAR_SpawnPoints;

    private int mIN_PrevMatchesNo;
    private int mIN_CurrMatchesNo;
    private List<GameObject> mLS_GO_CurrentMatchElements = new List<GameObject>();

    private int mIN_PrevAvailableMatches;
    private int mIN_CurrAvailableMatches;

    private int mIN_PlayersCount = 0;

    //[Header("Scenes To Load")]

    private enum ScenesToLoad
    {
        
    }

    [Header("Local Manager")]
    [SerializeField] private JB_GameManager mJB_GameManager;

    // Spawn Settings:

    void Awake()
    {
        if (instance== null)
        {
            instance = this;
        }

        else if (instance != null)
        {
            Destroy(gameObject);
        }

        StartMenu();
    }

    private void StartMenu()
    {
        // Load Menu Scene/
        OnLevelWasLoaded(1);
        DontDestroyOnLoad(this);

        // Disable MatchMaking panel.
        mRT_MainPanel.gameObject.SetActive(false);

        PhotonNetwork.automaticallySyncScene = true;
        mRO_RoomOpt = new RoomOptions();
        mTL_TypedLobby = TypedLobby.Default;

        // Set Room Options for the player;
        mRO_RoomOpt.IsVisible = true;
        mRO_RoomOpt.MaxPlayers = 4;

        //mRO_RoomOpt.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
        //{
        //    { "s", "Lobby" }
        //};
        //mRO_RoomOpt.CustomRoomPropertiesForLobby = new string[] { "s" }; // Makes level name accessible in a room.

        mSR_CurrentMatches = mSR_CurrentMatches.GetComponent<ScrollRect>();

        if (GetCurrMatches() == 0)
        {
            mSR_CurrentMatches.gameObject.SetActive(false);
            mTX_NoMatchAvailable.gameObject.SetActive(true);
            mTX_AvailableMatches.gameObject.SetActive(false);
        }

        else
        {
            mSR_CurrentMatches.gameObject.SetActive(true);
            mTX_NoMatchAvailable.gameObject.SetActive(false);
            mTX_AvailableMatches.gameObject.SetActive(true);
        }

        // Connect Just to the master server without connecting to a match.
        ConnectToMasterServer();
    }

    private void Update()
    {
        print("JC_PUN_Manager/ Update/ OnPlayersInRoomChanged() " + PhotonNetwork.playerList.Length);

        if (PhotonNetwork.insideLobby)
        {
            if (!PhotonNetwork.inRoom)
            {
                SetCurrMatches(PhotonNetwork.GetRoomList().Length);
            }
            
            //ViewCurrentMatches();
            if (!OnCurrentMatchesChanged())
            {
                return;
            }

            else
            {
                if (!PhotonNetwork.inRoom)
                {
                    SetPrevMatches(GetCurrMatches());
                    ViewCurrentMatches(); 
                }
            }
        }

        if (PhotonNetwork.inRoom)
        {
            //print("JC_PUNManager/OnPlayersInRoomChanged(): " + OnPlayersInRoomChanged());
            //print("JC_PUNManager/mIN_PlayersCount: " + mIN_PlayersCount);
            //print("JC_PUN_Manager/ Update/ OnPlayersInRoomChanged() " + PhotonNetwork.playerList.Length);

            if (OnPlayersInRoomChanged())
            {
                mIN_PlayersCount = PhotonNetwork.playerList.Length;
                //print("JC_PUN_Manager/ Update/ OnPlayersInRoomChanged() " + mIN_PlayersCount);
            }
        }
    }

    // Generate Room for the Match.
    void CreateMatch()
    {
        if (PhotonNetwork.insideLobby)
        {
            mST_MatchName = mIF_MatchName.text;
            PhotonNetwork.CreateRoom(mST_MatchName, mRO_RoomOpt, mTL_TypedLobby);
            print("JC_PUNManager/ CreateMatch()/ created match named: " + mST_MatchName);
        }
    }

    void JoinExistingMatch()
    {
        // Read selected match name and join it.

        if (PhotonNetwork.insideLobby)
        {
            if (PhotonNetwork.GetRoomList().Length > 0)
            {
                //JOIN MATCH
                PhotonNetwork.JoinRoom(mST_MatchName);
            }
        }
    }

    void ConnectToMasterServer()
    {
        if (PhotonNetwork.connected)
        {
            // Enable Match Making & Join Lobby.
            PhotonNetwork.JoinLobby(mTL_TypedLobby);
            print("JC_PUNManager/ ConnectJustToServer()/ PhotonNetwork.JoinLobby(mTL_TypedLobby).");
        }

        else
        {
            // Connect only to the master server.
            PhotonNetwork.ConnectUsingSettings(mST_GameVersion);
            print("JC_PUNManager/ ConnectJustToServer()/ PhotonNetwork.ConnectUsingSettings(mST_GameVersion)");
            PhotonNetwork.JoinLobby(mTL_TypedLobby);
            print("JC_PUNManager/ ConnectJustToServer()/ PhotonNetwork.JoinLobby(mTL_TypedLobby)");
        }
    }

    #region OnFunctions

    private bool OnCurrentMatchesChanged()
    {
        bool tBL_HasMatchListChanged = false;

        // If prev and current match are different update the canvas.
        if (mIN_CurrAvailableMatches != mIN_PrevAvailableMatches)
        {
            tBL_HasMatchListChanged = true;
        }

        return tBL_HasMatchListChanged;
    }

    private bool OnPlayersInRoomChanged()
    {
        bool tBL_HasPlayersNoChanged = false;

        if (PhotonNetwork.playerList.Length != mIN_PlayersCount)
        {
            tBL_HasPlayersNoChanged = true;
            return tBL_HasPlayersNoChanged;
        }

        else
        {
            tBL_HasPlayersNoChanged = false;
            return tBL_HasPlayersNoChanged;
        }
    }

    public override void OnJoinedRoom()
    {
        // Check if right room?
        Debug.Log("JC_PUNManager: OnJoinedRoom(), called by PUN. Now this client is in a room.");
        ViewCurrentMatches();

        PhotonNetwork.LoadLevel(2);

        SceneManager.sceneLoaded += OnSceneLoaded;
        OnLevelWasLoaded(2);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "PUNPayload_Test")
        {
            mAR_SpawnPoints = FindObjectsOfType<JC_PUNSpawner>();

            // Instantiate Player and set Don't destroy on load(In ControlPC script)
            InstantiatePC(mGO_PlayerPrefab);
        }
    }

    public void InstantiatePC(GameObject PlayerPrefab)
    {
        if (PUN_ControlPC.LocalPlayerInstance == null)
        {
            mAR_SpawnPoints = FindObjectsOfType<JC_PUNSpawner>();

            Vector3 position = new Vector3();

            foreach (JC_PUNSpawner spawner in mAR_SpawnPoints)
            {
                if (spawner.GetIsFree())
                {
                    position = spawner.transform.position;
                }
            }

            GameObject player = PhotonNetwork.Instantiate(PlayerPrefab.name, position, Quaternion.identity, 0);
            //PhotonNetwork.Instantiate(mGO_PlayerPrefab.name, position, Quaternion.identity, new object[] {(int)Team.Red});
            mIN_PlayersCount++;

            print("JC_PUNManager/ OnJoinedRoom()/ Created player with ID: " + player.GetPhotonView());
        }
    }


    // HAPPENS BEFORE ONSCENEWASLOADED WTF
    public void OnLevelWasLoaded(int level)
    {
        if (level == 1)
        {
            mRT_MainPanel.gameObject.SetActive(true);
        }

        if (level == 2)
        {
            if (!PhotonNetwork.inRoom)
            {
                Debug.LogError("JC_PUNManager/ OnLevelWasLoaded()/ NOT IN ROOM!!!");
                return;
            }

            mRT_MainPanel.gameObject.SetActive(false);

            if (GMinstance == null)
            {
                GMinstance = Instantiate(mJB_GameManager, transform);
                DontDestroyOnLoad(GMinstance);
            }

            // Round Robin or not!
            // Disable Shooting and other playing components for WaitingRoom!!
            // Check if this is the last player to be added to the game and start the actual game.
        }
    }

    public override void OnJoinedLobby()
    {
        print("JC_PUNManager/ OnJoinedLobby()/ Joined Lobby");

        // Enable MatchMenu.
        mRT_MainPanel.gameObject.SetActive(true);

        mBT_CreateMatch = mBT_CreateMatch.GetComponent<Button>();
        mBT_CreateMatch.onClick.AddListener(CreateMatch);

        SetPrevMatches(PhotonNetwork.GetRoomList().Length);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("JC_PUNManager: OnConnectedToMaster(), called by PUN");

        if (!PhotonNetwork.insideLobby)
        {
            PhotonNetwork.JoinLobby(mTL_TypedLobby);
        }
    }

    #endregion

    // MAKE SURE THIS HAPPENS ONLY ON GETROOMLISTCHANGED()!!!!!!!!
    // Probably needs to be synced [PUNRpc]. --> NO it doesn't see documentation about GetRoom()
    void ViewCurrentMatches()
    {
        if (PhotonNetwork.insideLobby)
        {
            //print("JC_PUNManager/ ViewCurrentMatches()/ PhotonNetwork.inRoom: " + PhotonNetwork.inRoom);
            //print("JC_PUNManager/ ViewCurrentMatches()/ PhotonNetwork.insideLobby: " + PhotonNetwork.insideLobby);
            //print("JC_PUNManager/ ViewCurrentMatches()/ PhotonNetwork.playerList.Length: " + PhotonNetwork.playerList.Length);

            if (GetCurrMatches() == 0)
            {
                mSR_CurrentMatches.gameObject.SetActive(false);
                mTX_NoMatchAvailable.gameObject.SetActive(true);
                mTX_AvailableMatches.gameObject.SetActive(false);
                print("JC_PUNManager/ ViewCurrentMatches()/" + PhotonNetwork.GetRoomList().Length);
            }

            // If there are current Matches, get the Scroll Bar and add the relative matches.
            else if (GetCurrMatches() > 0)
            {
                mSR_CurrentMatches.gameObject.SetActive(true);
                mTX_NoMatchAvailable.gameObject.SetActive(false);
                mTX_AvailableMatches.gameObject.SetActive(true);

                //Empty MatchLIst
                RectTransform[] tMatchElements = mSR_CurrentMatches.GetComponentsInChildren<RectTransform>();

                foreach (RectTransform item in tMatchElements)
                {
                    if (item.GetComponent<JC_PUNMatchElement>())
                    {
                        Destroy(item.gameObject);
                    }
                }

                int index = 0;

                //Update View;
                // Add to List of Match Panel element
                foreach (RoomInfo Match in PhotonNetwork.GetRoomList())
                {
                    Vector3 vOffset = new Vector3(0, 180, 0);
                    RectTransform vMatchPElement = Instantiate(mRT_MatchPanelElement, mSR_CurrentMatches.viewport.transform);
                    vMatchPElement.transform.position = vMatchPElement.transform.position + new Vector3(0, (float)-42 * index, 0);
                    vMatchPElement.GetComponent<JC_PUNMatchElement>().mST_MatchName = Match.Name;

                    if (vMatchPElement.GetChild(0).GetComponent<Button>() == null)
                    {
                        print("NO JOIN BUTTON FOUND");
                    }

                    vMatchPElement.GetChild(1).GetComponent<Text>().text = Match.PlayerCount + " / " + Match.MaxPlayers;
                    vMatchPElement.GetChild(2).GetComponent<Text>().text = Match.Name;
                    GetMatchName(Match.Name);
                    vMatchPElement.GetChild(0).GetComponent<Button>().onClick.AddListener(SetMatchName);
                    vMatchPElement.GetChild(0).GetComponent<Button>().onClick.AddListener(JoinExistingMatch);

                    index++;  
                }

                print("JC_PUNManager/ ViewCurrentMatches()/" + PhotonNetwork.GetRoomList().Length);
            }

            //// Show No Matches Available.
            //else if (PhotonNetwork.GetRoomList().Length == 0)
            //{
            //    mSR_CurrentMatches.gameObject.SetActive(false);
            //    mTX_NoMatchAvailable.gameObject.SetActive(true);
            //    print("JC_PUNManager/ ViewCurrentMatches()/" + PhotonNetwork.GetRoomList().Length);
            //}
        }

        else
        {
            Debug.Log("JC_PUNManager/ ViewCurrentMatches()/ NOT IN LOBBY");
        }
    }

    #region Getters & Setters

    private int GetCurrMatches()
    {
        return mIN_CurrAvailableMatches;
    }

    private void SetCurrMatches(int vCurr)
    {
        mIN_CurrAvailableMatches = vCurr;
    }

    private int GetPrevMatches()
    {
        return mIN_PrevAvailableMatches;
    }

    private void SetPrevMatches(int vPrev)
    {
        vPrev = mIN_CurrAvailableMatches;
        mIN_PrevAvailableMatches = mIN_CurrAvailableMatches;
    }

    private void GetMatchName(string vName)
    {
        mST_JoinMatchName = vName;
    }

    private void SetMatchName()
    {
        if (mST_JoinMatchName != mST_MatchName)
        {
            mST_MatchName = mST_JoinMatchName;
        }
    }

    public JC_PUNSpawner[] GetSpawnPoints()
    {
        return mAR_SpawnPoints;
    }

    #endregion
}
