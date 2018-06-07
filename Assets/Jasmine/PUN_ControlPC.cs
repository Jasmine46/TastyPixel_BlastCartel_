using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct MovementMods
{
    public MovementMods(Vector3 direction, float startTime, float removeTime, bool fade, bool groundClear, bool gravReset)
    {
        modDirection = currentVector = direction;
        modStartTime = startTime;
        modRemoveTime = removeTime;
        modFadesOut = fade;
        resetGravityWhileActive = gravReset;
        removeWhenGrounded = groundClear;
    }

    public Vector3 modDirection;
    public Vector3 currentVector;
    public float modStartTime;
    public float modRemoveTime;
    public bool modFadesOut;
    public bool removeWhenGrounded;
    public bool resetGravityWhileActive;
}

public class PUN_ControlPC : Photon.PunBehaviour, IPunObservable
{
    #region Gameplay Variables

    // Animation
    private Animator anim;
    public AnimationCurve exponentialCurveUp;

    // Basic Movement
    [HideInInspector]
    public CharacterController cc;
    private Vector3 moveDirection;
    private float speed;
    [Header("Basic Movement")]
    public float baseSpeed;
    public float sprintMultiplier = 1;
    public float strafeMultiplier = .8f;
    public float airBaseSpeed;
    public bool isGrounded;
    private bool isFalling;
    [HideInInspector]
    public List<MovementMods> movementModifiers = new List<MovementMods>();

    // Jumping
    [Header("Jumping")]
    public float jumpTimeLength = 1;
    public float jumpHeight = 2;
    private bool isJumping;
    private float jumpTimer = 0;

    // Abilities
    [Header("Abilities")]
    public JB_MovementAbility currentMovementAbility;

    /// <summary>
    /// CHANGE HOOK SCRIPTS TO PUN SCRIPTS!!!
    /// </summary>

    [HideInInspector]
    public bool movedByAbility;

    // Rigidbody & Physics
    private bool wasStopped;
    public float gravity = 1;
    [HideInInspector]
    public float appliedGravity;

    // Camera
    [Header("Camera")]
    public Transform cameraContianer;
    [HideInInspector]
    public Camera cam;
    public Transform head;
    public float yRotationSpeed = 45;
    public float xRotationSpeed = 45;
    private float yRotation;
    private float xRotation;

    // UI
    [Header("UI")]
    public BaseHUD baseHudPrefab;
    [HideInInspector]
    public BaseHUD baseHud;

    // Networking
    //[SyncVar]
    public int referenceID;
    private float netStep = 0;
    private Vector3 nextPos;
    private Quaternion nextRot;

    //[SyncVar]
    private int animState;  // do I need this or do we use built in animator syncing?
    private enum AnimationStates
    {
        Idle,
        Walking,
        Running,
        Jumping,
    }

    private AnimationStates pcAnimationState;

    // Stats
    [Header("Stats")]
    public int maxHealth = 100;
    //[HideInInspector]
    public int health = 100;
    public JB_GameManager.WeightClass playerWeight;

    // Weapons
    [Header("Weapons")]
    public JB_GameManager.AllWeapons primaryWeapon;
    public JB_GameManager.AllWeapons secondaryWeapon;
    public JB_GameManager.AllWeapons tertiaryWeapon;
    public Transform barrel;
    [HideInInspector]
    public JB_Weapon selectedWeapon;
    [HideInInspector]
    public JB_Weapon currentPrimary;
    [HideInInspector]
    public JB_Weapon currentSecondary;
    [HideInInspector]
    public JB_Weapon currentTertiary;
    public bool firedPrimary;

    // Aesthetics
    [Header("Aesthetics")]
    public SkinnedMeshRenderer playerBodyMesh;

    public Texture[] textureChoices;
    //[SyncVar]
    int tempTextureChoice;

    // Hitboxes
    [Header("Hitboxes")]
    public HitboxLink[] allPlayerHitBoxes;

    public static GameObject LocalPlayerInstance;

    #endregion

    #region PhotonSync temp Variables

    private JC_PUNManager mPUN_Manager;
    private bool mPUN_EnableMovPrediction = false;

    private Vector3 mPUN_Position;
    private Quaternion mPUN_Rotation;
    private Vector3 mPUN_Velocity;
    private double mPUN_LastNetworkReceivedTime;
    private Vector3 mV3_PUNAmmoPos;
    private int mIN_oldHealth;
    private PhotonView[] allViews = new PhotonView[10];

    #endregion

    #region Setup Functions

    private void Awake()
    {
        if (photonView.isMine)
        {
            LocalPlayerInstance = gameObject;
        }

        DontDestroyOnLoad(gameObject);

        mPUN_Manager = FindObjectOfType<JC_PUNManager>().GetComponent<JC_PUNManager>();
    }

    void SyncGameplayVariables()
    {
        //referenceID;
        //netStep = 0;
        //nextPos;
        //nextRot;
        //animState;
        //pcAnimationState;
        ////OnHealthChanged();
        //tempTextureChoice;
    }

    //Substitute to OnStartLocalPlayer()
    private void Start()
    {
        if (!photonView.isMine)
        {
            return;
        }

        cameraContianer.GetChild(0).gameObject.SetActive(true);
        cam = cameraContianer.GetComponentInChildren<Camera>();

        if (!cameraContianer.GetChild(0).gameObject.activeInHierarchy && cam == null)
        {
            Debug.LogError("PUN_ControlPC/ Start()/ !!CAMERA NOT FOUND!!");
            return;
        }

        baseHud = Instantiate(baseHudPrefab);
        baseHud.PUNpc = this;
        DontDestroyOnLoad(baseHud);
        health = maxHealth;

        if (baseHud == null) // PORCO DIO
        {
            Debug.LogError("PUN_ControlPC/ Start()/ !!HUD NOT FOUND!!");
            return;
        }

        yRotation = transform.localEulerAngles.y;
        xRotation = cam.transform.localEulerAngles.x;
        wasStopped = true;
        appliedGravity = gravity / 2;

        Application.runInBackground = true;

        anim = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();
        currentMovementAbility.PUNpc = this;
        ChangeOwnHitboxes();
        SpawnWeapon();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Control Photon send and send on serialize
        //PhotonNetwork.sendRate = 20;
        //PhotonNetwork.sendRateOnSerialize = 10;

        // Control how data is being streamed from the player(or team)
        //PhotonNetwork.SetSendingEnabled((byte)1, false);
        //PhotonNetwork.SetReceivingEnabled((byte)1, false);
    }

    void ChangeOwnHitboxes()
    {
        foreach (HitboxLink item in allPlayerHitBoxes)
        {
            item.gameObject.layer = 12;
        }
    }

    void SpawnWeapon()
    {
        if (photonView.isMine)
        {
            GameObject GO_CurrentPrimary = PhotonNetwork.Instantiate(JB_GameManager.gm.EquipWeapon(primaryWeapon).name, transform.position, Quaternion.identity, 0);
            GO_CurrentPrimary.transform.SetParent(gameObject.transform);
            DontDestroyOnLoad(GO_CurrentPrimary);
            currentPrimary = GO_CurrentPrimary.GetComponent<JB_Weapon>();
            currentPrimary.PUNpc = this;
            selectedWeapon = currentPrimary;
        }
    }

    //void StartMoveToSpawn()
    //{
    //    foreach (JC_PUNSpawner spawner in mPUN_Manager.GetSpawnPoints())
    //    {
    //        if (spawner.GetIsFree())
    //        {
    //            transform.position = spawner.gameObject.transform.position;
    //            spawner.SetIsFree(false);
    //            return;
    //        }
    //    }
    //}

    #endregion

    #region Updates and Inputs

    void Update()
    {
        if (photonView.isMine)
        {
            //Double check if UI is there, don't like this fix. (!)
            if (baseHud == null)
            {
                print("PUN_ControlPC/ Update()/ BASEHUD NULL!!!!");

                baseHud = Instantiate(baseHudPrefab);
                baseHud.PUNpc = this; //baseHud.pc = this;
                DontDestroyOnLoad(baseHud);

                print("PUN_ControlPC/ Update()/ found? " + baseHud.isActiveAndEnabled);
            }

            //if (mPUN_Manager.mBL_HasRoomSceneLoaded)
            //{
            //    StartMoveToSpawn();
            //    mPUN_Manager.mBL_HasRoomSceneLoaded = false;
            //}

            GetPlayerInput();
            MovePC();

            //netStep += Time.deltaTime;
            //if (netStep >= GetNetworkSendInterval())
            //{
            //    netStep = 0;
            //    CmdCallSync(transform.position, transform.rotation, cc.velocity);
            //}

            if (Input.GetKeyDown(KeyCode.Escape))   //show cursor in editor
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        else
        {
            //!!!!! CONVERT TO PHOTON!!!!!
            //LerpClient();
            //UpdatePCPosition();
        }
    }

    void FixedUpdate()
    {
        if (!photonView.isMine)
        {
            return;
        }

        CheckForGround();
    }

    private void UpdatePCPosition()
    {
        if (mPUN_EnableMovPrediction)
        {
            // PREDICTION! Could use last input registered too!
            // Network time variables.
            float pingInSecs = (float)PhotonNetwork.GetPing() * 0.001f;
            float timeSinceLastUpdate = (float)(PhotonNetwork.time - mPUN_LastNetworkReceivedTime);
            float totalTimePast = pingInSecs + timeSinceLastUpdate;

            // Predicted Position!
            Vector3 exterpolatedTargetPosition = mPUN_Position + cc.velocity * totalTimePast;
            Vector3 newPos = Vector3.Lerp(transform.position, exterpolatedTargetPosition, .5f);

            // Change Accuracy Maybe?
            if (Vector3.Distance(transform.position, exterpolatedTargetPosition) > 0.5f) //<---
            {
                newPos = exterpolatedTargetPosition;
            }

            transform.position = newPos;
        }

        else
        {
            // No Prediction involved;
            transform.position = Vector3.Lerp(transform.position, mPUN_Position, .5f);
            transform.rotation = Quaternion.Lerp(transform.rotation, mPUN_Rotation, .5f);
        }
    }

    void GetPlayerInput()
    {
        // Keyboard input
        moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        moveDirection.Normalize();
        moveDirection.x *= strafeMultiplier; // slower strafe
        moveDirection = transform.TransformDirection(moveDirection);
        speed = 0;

        if (Mathf.Abs(moveDirection.x) != 0 || Mathf.Abs(moveDirection.z) != 0)
        {
            if (Mathf.Abs(moveDirection.x) == 1 || Mathf.Abs(moveDirection.z) == 1)
            {
                wasStopped = false;
            }

            pcAnimationState = AnimationStates.Running;
            speed = 1;

            if (sprintMultiplier != 0 && Input.GetButton("Sprint"))  // if PC is sprinting
            {
                speed *= sprintMultiplier;
                pcAnimationState = AnimationStates.Running;
            }
        }

        anim.SetFloat("Speed", speed * 2);

        // Movement Ability
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            currentMovementAbility.UseAbility(cam.transform.TransformDirection(Vector3.forward));
        }

        // Aerial
        if (movedByAbility && Input.GetButtonDown("Jump"))
        {
            currentMovementAbility.CancelAbility();
        }

        else if (isGrounded && !isJumping && Input.GetButtonDown("Jump"))
        {
            isJumping = true;
            anim.Play("JumpInitial");
        }

        // Mouse input
        yRotation += Input.GetAxis("Mouse X") * yRotationSpeed * Time.deltaTime;
        xRotation -= Input.GetAxis("Mouse Y") * xRotationSpeed * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (xRotation != cam.transform.eulerAngles.x || yRotation != transform.eulerAngles.y)
        {
            cam.transform.localEulerAngles = new Vector3(xRotation, 0, 0);
            transform.localEulerAngles = new Vector3(0, yRotation, 0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            TakeDamage(10);
            CheckHealth();
        }

        CheckWeaponInput();
    }

    void CheckWeaponInput()
    {
        // Weapon input
        if (Input.GetKeyDown(KeyCode.R))                    // reload
        {
            selectedWeapon.TryStartReload();
        }

        else if (Input.GetMouseButton(0))                   // if player uses primary fire
        {
            selectedWeapon.FireWeapon();
        }

        else if (Input.GetMouseButtonUp(0))                 // if player lets go of primary fire button
        {
            selectedWeapon.StoppedFiring();
        }

        else if (Input.GetMouseButton(1))                   // if player uses secondary fire
        {
            selectedWeapon.FireWeaponSecondary();
        }

        else if (Input.GetMouseButtonUp(1))                 // if player lets go of secondary fire button
        {
            selectedWeapon.StoppedFiring();
        }
        //
    }

    sbyte RoundToLargest(float inp)
    {
        if (inp > 0)
        {
            return 1;
        }

        else if (inp < 0)
        {
            return -1;
        }

        return 0;
    }

    #endregion

    void MovePC()
    {
        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            Debug.LogError("PUN_ControlPC/ MovePC()/ photonView NOT THERE!!!");
            return;
        }

        if (!movedByAbility)
        {
            if (isGrounded)
            {
                if (Mathf.Abs(moveDirection.x) != 0 || Mathf.Abs(moveDirection.z) != 0) // if there's some input
                {
                    moveDirection *= baseSpeed * speed;
                }

                else
                {
                    pcAnimationState = AnimationStates.Idle;
                }
            }

            else
            {
                if (Mathf.Abs(moveDirection.x) != 0 || Mathf.Abs(moveDirection.z) != 0) // if there's some input
                {
                    moveDirection *= airBaseSpeed * speed;
                }
            }

            ApplyJump();
            ApplyGravity();
            ApplyMovementModifiers();

            cc.Move(moveDirection * Time.deltaTime);
            moveDirection = Vector3.zero;
            if (cc.velocity == Vector3.zero) wasStopped = true;
        }

        ResetGravityFromModifier();
    }

    void ApplyJump()
    {
        if (isJumping)
        {
            jumpTimer += Time.deltaTime;
            moveDirection += Vector3.up * jumpHeight * (1 - (jumpTimer / jumpTimeLength));

            if (jumpTimer >= jumpTimeLength)
            {
                isJumping = false;
                appliedGravity = jumpTimer = 0;
            }
        }
    }

    void ApplyGravity()
    {
        if (!isGrounded)
        {
            if (!isFalling)
            {
                isFalling = true;
                movementModifiers.Add(new MovementMods(cc.velocity / 2, Time.time, Time.time + 1, true, true, false));
            }
        }

        if (!isJumping)
        {
            moveDirection += Vector3.down * appliedGravity;
            appliedGravity += gravity * Time.deltaTime;
        }
    }

    bool CheckForGround()
    {
        int layermask = 1 << 8;
        RaycastHit hit;

        if (Physics.SphereCast(transform.position + Vector3.up, .5f, Vector3.down, out hit, .6f, layermask))
        {
            appliedGravity = gravity / 3;
            isFalling = false;
            GroundClearMoveMods();
            return isGrounded = true;
        }

        else
        {
            return isGrounded = false;
        }
    }

    #region Movement Mods

    void ApplyMovementModifiers()   // applies movement modifiers (e.g. motion retained when walking over an edge, or from an explosion)
    {
        for (int i = movementModifiers.Count - 1; i > -1; i--)
        {
            if (Time.time >= movementModifiers[i].modStartTime) // if mod effect is to start
            {
                if (Time.time >= movementModifiers[i].modRemoveTime)    // if the movement modifier has timed out
                {
                    movementModifiers.RemoveAt(i);
                }

                else
                {
                    if (movementModifiers[i].modFadesOut)   // if the mod force fades out over time reduce it's force
                    {
                        moveDirection += movementModifiers[i].modDirection *
                            (1 - (Time.time - movementModifiers[i].modStartTime) / (movementModifiers[i].modRemoveTime - movementModifiers[i].modStartTime));
                    }

                    else
                    {
                        moveDirection += movementModifiers[i].currentVector;
                    }
                }
            }
        }
    }

    void ResetGravityFromModifier()
    {
        for (int i = movementModifiers.Count - 1; i > -1; i--)
        {
            if (movementModifiers[i].resetGravityWhileActive)
            {
                appliedGravity = 0;
                return;
            }
        }
    }

    void GroundClearMoveMods()
    {
        for (int i = movementModifiers.Count - 1; i > -1; i--)
        {
            if (movementModifiers[i].removeWhenGrounded)
            {
                movementModifiers.RemoveAt(i);
            }
        }
    }

    #endregion

    void WeaponFire()
    {
        currentPrimary.FireWeapon();
    }

    #region Shooting & Health 

    // Cannot Serialize GameObject type through Photon, Use PlayerID.

    [PunRPC]
    public void ShootPC(int playerHitID, int dmg, bool isHeadShot) //(GameObject hitPoint, int dmg, int layer)
    {
        // !!!Figure out PhotonPlayer!!!

        GameObject hitPoint = FindGameObjectFromPhotonID(playerHitID);

        if (hitPoint)
        {
            print("PUN_ControlPC/ Shot GameObject with PhotonID: " + playerHitID);

            HitboxLink hbl = hitPoint.GetComponent<HitboxLink>();

            //if (hbl)
            //{
            if (isHeadShot) //(layer == 10)    // If it's a headshot
            {
                //DoDamage(hbl.pc.gameObject, dmg * 2);

                photonView.RPC("DoDamage", PhotonTargets.AllBufferedViaServer, new object[] { playerHitID, dmg * 2 }); //{ hbl.pc.gameObject, dmg * 2 });
                print("PUN_PCControl/ DoDamage() RPC DoDamage() x 2");
            }

            else
            {
                //DoDamage(hbl.pc.gameObject, dmg);

                photonView.RPC("DoDamage", PhotonTargets.AllBufferedViaServer, new object[] { playerHitID, dmg }); //{ hbl.pc.gameObject, dmg });
                print("PUN_PCControl/ DoDamage() RPC DoDamage()");
            }
            //}
        }
    }

    [PunRPC]
    void DoDamage(int HitID, int dmg) //(GameObject hitPC, int dmg)
    {
        GameObject hitPC = FindGameObjectFromPhotonID(HitID);

        print("PUN_PCControl/ Do Damage to " + hitPC.GetComponent<PhotonView>().viewID);

        //print("PUN_PCControl/ Do Damage to " + hitPC.GetComponent<PhotonView>().viewID + " with health " + hitPC.GetComponent<PUN_ControlPC>().health);

        if (hitPC)
        {
            PUN_ControlPC _pc = hitPC.GetComponent<PUN_ControlPC>();

            if (_pc)
            {
                _pc.health -= dmg;
                baseHud.SetHealth(health);
                CheckHealth();

                ////_pc.TakeDamage(dmg);    

                //_pc.GetComponent<PhotonView>().RPC("TakeDamage", PhotonTargets.AllBufferedViaServer, new object[] { health });
                //print("PUN_PCControl/ DoDamage() RPC --> TakeDamage");
            }

            // Other PlayerID.
            else
            {
                print("No PC");
            }
        }
    }

    void TakeDamage(int dmg)
    {
        print("PUN_PCControl/ TakeDamage()");
        health -= dmg;
    }

    private Vector3 GetAmmoSpawnPosition()
    {
        return mV3_PUNAmmoPos;
    }

    private void OnHealthChanged()
    {
        print("PUNControlPC/ SerializeHealth()/ OnHealthChanged()!!!!!!" + health + mIN_oldHealth);
        mIN_oldHealth = health;
        baseHud.SetHealth(health);
    }

    [PunRPC]
    public void FireAmmo(JB_GameManager.AllWeapons weap, JB_GameManager.AttackTypes aType, Vector3 velocity, int damage)
    {
        JB_Ammo _ammo = JB_GameManager.gm.GetAmmo(weap, aType);
        if (!_ammo) return;

        var ammo = Instantiate(_ammo, barrel.position, Quaternion.identity);
        ammo.rb.velocity = velocity;
        ammo.damage = damage;

        mV3_PUNAmmoPos = ammo.transform.position;

        //NetworkServer.Spawn(ammo.gameObject);
        PhotonNetwork.Instantiate(ammo.gameObject.name, transform.position, Quaternion.identity, 0);
    }

    #endregion

    private GameObject FindGameObjectFromPhotonID(int playerID)
    {
        GameObject player = null;

        PUN_ControlPC[] allPlayers = FindObjectsOfType<PUN_ControlPC>();

        if (allPlayers.Length != 0)
        {
            for (int i = 0; i < allPlayers.Length; i++)
            {
                allViews[i] = allPlayers[i].GetComponent<PhotonView>();
            }
        }

        // If the PhotonManager is there and the list is not empty.
        if (mPUN_Manager != null && allViews.Length != 0)
        {
            foreach (PhotonView Player in allViews)
            {
                // If it's the right player and not this player.
                if (Player.viewID == playerID)
                {
                    player = Player.gameObject;
                    print("PUN_ControlPC/ FindGameObjectFromPhotonID" + player.name);

                    return player;
                }
            }

            return null;
        }

        else
        {
            return null;
        }
    }

    void Respawn() //!!!!
    {
        gameObject.SetActive(false);
    }                

    // Finish
    void CheckHealth()
    {
        if (!photonView.isMine)
        {
            return;
        }

        if (health <= 0)
        {
            if (baseHud) baseHud.OnRespawn();
            //Respawn();
        }

        baseHud.SetHealth(health);

        print("PUN_Control/ CheckHealth: " + baseHud.health.value);
    }

    #region Serialization

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //print("Serialising health");
        SerializeHealth(stream, info);
        //SerializePosition(stream, info);
        // Same for movement and shooting
    }

    void SerializeHealth(PhotonStream stream, PhotonMessageInfo info)
    {
        // Update Health
        if (stream.isWriting)
        {
            //print("PUNControlPC/ SerializeHealth()/ stream.isWriting");
            stream.SendNext(health);
            //print("PUNControlPC/ SerializeHealth()/ stream.SendNext(health)");
        }

        else
        {
            //print("PUNControlPC/ SerializeHealth()/ stream.isReading" );

            mIN_oldHealth = health;
            health = (int)stream.ReceiveNext();

            if (health != mIN_oldHealth)
            {
                //print("PUNControlPC/ SerializeHealth()/ OnHealthChanged()");
                //Update Health
                OnHealthChanged();
            }
        }

        //OR
        //stream.Serialize(ref health);
    }

    // NEEDS FIXING
    //void SerializePosition(PhotonStream stream, PhotonMessageInfo info)
    //{
    //    if (stream.isWriting)
    //    {
    //        stream.SendNext(transform.position);
    //        stream.SendNext(transform.rotation);
    //        stream.SendNext(cc.velocity);
    //    }

    //    else
    //    {
    //        // INTERPOLATE BETWEEN CURRENT AND THESE VALUES!
    //        mPUN_Position = (Vector3)stream.ReceiveNext();
    //        mPUN_Rotation = (Quaternion)stream.ReceiveNext();
    //        mPUN_Velocity = (Vector3)stream.ReceiveNext();

    //        mPUN_LastNetworkReceivedTime = info.timestamp;                           
    //    }
    //}

    #endregion
}
