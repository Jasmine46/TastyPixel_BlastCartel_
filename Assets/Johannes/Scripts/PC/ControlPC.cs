using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Huge help from https://github.com/GenaSG/UnityUnetMovement for server-authoritative movement
/// </summary>

public struct MovementMod
{
    public MovementMod(Vector3 direction, float startTime, float removeTime, bool fade, bool groundClear, bool gravReset)
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

[NetworkSettings(channel = 1, sendInterval = 0.0333f)]
public class ControlPC : NetworkBehaviour
{

    # region Gameplay Variables

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
    public List<MovementMod> movementModifiers = new List<MovementMod>();

    // Jumping
    [Header("Jumping")]
    public float jumpTimeLength = 1;
    public float jumpHeight = 2;
    private bool isJumping;
    private float jumpTimer = 0;

    // Abilities
    [Header ("Abilities")]
    public JB_MovementAbility currentMovementAbility;
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
    [SyncVar]
    public int referenceID;
    private float netStep = 0;
    private Vector3 nextPos;
    private Quaternion nextRot;

    [SyncVar]
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
    [SyncVar(hook = "OnHealthChange")]
    [HideInInspector]
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
    [SyncVar]
    int tempTextureChoice;

    // Hitboxes
    [Header("Hitboxes")]
    public HitboxLink[] allPlayerHitBoxes;

    #endregion

    #region Setup Functions

    public override void OnStartLocalPlayer()
    {
        cameraContianer.GetChild(0).gameObject.SetActive(true);
        cam = cameraContianer.GetComponentInChildren<Camera>();
        baseHud = Instantiate(baseHudPrefab);
        baseHud.pc = this;
        health = maxHealth;
        yRotation = transform.localEulerAngles.y;
        xRotation = cam.transform.localEulerAngles.x;
        wasStopped = true;
        appliedGravity = gravity / 2;
        Application.runInBackground = true;
        anim = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();
        currentMovementAbility.pc = this;
        ChangeOwnHitboxes();
        SpawnWeapon();
        CmdChooseTexture();
        CmdCallSync(transform.position, transform.rotation, cc.velocity);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    [Command]
    void CmdChooseTexture()
    {
        tempTextureChoice = Random.Range(0, textureChoices.Length);
        RpcChooseTexture();
    }

    void RpcChooseTexture()
    {
        playerBodyMesh.materials[0].SetTexture("_MainTex", textureChoices[tempTextureChoice]);
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
        currentPrimary = Instantiate(JB_GameManager.gm.EquipWeapon(primaryWeapon), this.transform);
        currentPrimary.pc = this;
        //currentSecondary = Instantiate(JB_GameManager.gm.EquipWeapon(secondaryWeapon), this.transform);
        //currentSecondary.pc = this;
        //currentTertiary = Instantiate(JB_GameManager.gm.EquipWeapon(tertiaryWeapon), this.transform);
        //currentTertiary.pc = this;
        selectedWeapon = currentPrimary;
    }

    #endregion

    #region Updates and Inputs

    void Update()
    {
        if (isLocalPlayer)
        {
            GetPlayerInput();
            MovePC();

            netStep += Time.deltaTime;
            if (netStep >= GetNetworkSendInterval())
            {
                netStep = 0;
                CmdCallSync(transform.position, transform.rotation, cc.velocity);
            }

            if (Input.GetKeyDown(KeyCode.Escape))   //show cursor in editor
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        else
        {
            LerpClient();
        }
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        CheckForGround();
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

        CheckWeaponInput();
    }

    void CheckWeaponInput()
    {
        // Weapon input
        if (Input.GetKeyDown(KeyCode.R))                     // reload
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

    void ResetGravity()
    {
        appliedGravity = 0;
    }

    void MovePC()
    {
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
                movementModifiers.Add(new MovementMod(cc.velocity / 2, Time.time, Time.time + 1, true, true, false));
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

    #region Network Functions

    [Client]
    public void ShootPC(GameObject hitPoint, int dmg, int layer)
    {
        if (hitPoint)
        {
            HitboxLink hbl = hitPoint.GetComponent<HitboxLink>();
            if (hbl)
            {
                if (layer == 10)    // if it's a headshot
                {
                    CmdDoDamage(hbl.pc.gameObject, dmg * 2);
                }
                else
                {
                    CmdDoDamage(hbl.pc.gameObject, dmg);
                }
            }
        }
    }

    [Command]
    public void CmdFireAmmo(JB_GameManager.AllWeapons weap, JB_GameManager.AttackTypes aType, Vector3 velocity, int damage)
    {
        JB_Ammo _ammo = JB_GameManager.gm.GetAmmo(weap, aType);
        if (!_ammo) return;

        var ammo = Instantiate(_ammo, barrel.position, Quaternion.identity);
        ammo.rb.velocity = velocity;
        ammo.damage = damage;

        NetworkServer.Spawn(ammo.gameObject);
    }

    [Command]
    void CmdDoDamage(GameObject hitPC, int dmg)
    {
        if (hitPC)
        {
            ControlPC _pc = hitPC.GetComponent<ControlPC>();
            if (_pc)
            {
                _pc.RpcTakeDamage(dmg);
            }
            else
            {
                print("No PC");
            }
        }
    }

    [Command]
    public void CmdTakeDamage(int dmg)
    {
        RpcTakeDamage(dmg);
    }

    [ClientRpc]
    void RpcTakeDamage(int dmg)
    {
        health -= dmg;
    }

    [Command]
    public void CmdEnterKillzone()
    {
        RpcEnterKillzone();
    }

    [ClientRpc]
    void RpcEnterKillzone()
    {
        health = 0;
    }

    void OnHealthChange(int newHealth)
    {
        health = newHealth;
        CheckHealth();
    }

    [Command]
    void CmdRespawn()
    {
        Transform spawn = NetworkManager.singleton.GetStartPosition();
        GameObject newPlayer = (GameObject)Instantiate(NetworkManager.singleton.playerPrefab, spawn.position, spawn.rotation);
        NetworkServer.Destroy(this.gameObject);
        NetworkServer.ReplacePlayerForConnection(this.connectionToClient, newPlayer, this.playerControllerId);
    }

    [Command]
    void CmdCallSync(Vector3 position, Quaternion rotation, Vector3 velocity)
    {
        RpcSendNextPos(position, rotation);
    }

    void LerpClient()
    {
        if (!isLocalPlayer)
        {
            transform.position = Vector3.Lerp(transform.position, nextPos, .5f);
            transform.rotation = Quaternion.Lerp(transform.rotation, nextRot, .5f);
        }
    }
    
    [ClientRpc]
    void RpcSendNextPos(Vector3 _nextPos, Quaternion _nextRot)
    {
        nextPos = _nextPos;
        nextRot = _nextRot;
    }

    [ClientRpc]
    public void RpcChangeAllyLayer(GameObject allyPC)
    {
        ControlPC _ally = allyPC.GetComponent<ControlPC>();
        if (_ally)
        {
            foreach (HitboxLink hitbox in _ally.allPlayerHitBoxes)
            {
                // change ally's hitboxes to the ally layer
                hitbox.gameObject.layer = JB_GameManager.gm.allyLayer;
            }
        }
    }

    #endregion

    void CheckHealth()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        if (health <= 0)
        {
            if (baseHud) baseHud.OnRespawn();
            CmdRespawn();
            CmdCallSync(transform.position, transform.rotation, cc.velocity);
        }
        baseHud.SetHealth(health);
    }
}
