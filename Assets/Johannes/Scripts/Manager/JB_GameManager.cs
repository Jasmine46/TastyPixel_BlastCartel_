using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class JB_GameManager : MonoBehaviour
{
    #region Variables

    #region Player Variables
    public enum WeightClass
    {
        light,
        medium,
        heavy
    }
    #endregion

    #region Weapon Variables
    //Weapons
    public static JB_GameManager gm;
    [Header("All Weapon Types")]
    [SerializeField]
    private JB_Weapon_AssaultRifle weaponAssaultRifle;
    [SerializeField]
    private JB_Weapon_NoobTube weaponNoobTube;

    public enum AllWeapons
    {
        None,
        AssaultRifle,
        NoobTube
    }

    public enum AttackTypes
    {
        Primary,
        Secondary,
        Tertiary
    }

    //
    // Ammo
    [Header("All Spawnable Ammo Types")]
    [SerializeField]
    private JB_Ammo_NoobTube ammoNoobTubePrimary;
    [SerializeField]
    private JB_Ammo_NoobTubeSecondary ammoNoobTubeSecondary;
    //
    #endregion

    #region Layers
    public LayerMask terrainLayer;
    public LayerMask weaponLayer;
    public LayerMask enemyLayer;
    public LayerMask allyLayer;
    public LayerMask selfLayer;

    #endregion

    #region Scene Objects

    public NetworkStartPosition[] spawnPoints;

    #endregion

    #region Current Game Variables

    public int numberOfTeams;

    #endregion

    #endregion

    void Awake()
    {
        if (gm)
        {
            Destroy(this);
        }

        else
        {
            gm = this;
        }

        //spawnPoints = FindObjectsOfType<NetworkStartPosition>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public JB_Weapon EquipWeapon(AllWeapons weap)
    {
        switch (weap)
        {
            case AllWeapons.None:
                return null;
            case AllWeapons.AssaultRifle:
                return weaponAssaultRifle;
            case AllWeapons.NoobTube:
                return weaponNoobTube;
            default:
                return weaponAssaultRifle;
        }
    }

    public JB_Ammo GetAmmo(AllWeapons weap, AttackTypes aType)
    {
        switch (weap)
        {
            case AllWeapons.None:
                return null;
            case AllWeapons.AssaultRifle:
                return null;
            case AllWeapons.NoobTube:
                if (aType == AttackTypes.Primary)
                {
                    return ammoNoobTubePrimary;
                }
                else if (aType == AttackTypes.Secondary)
                {
                    return ammoNoobTubeSecondary;
                }
                else
                {
                    return null;
                }
            default:
                return null;
        }
    }
}
