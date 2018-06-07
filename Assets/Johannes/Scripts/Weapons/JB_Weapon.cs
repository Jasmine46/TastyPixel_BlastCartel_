using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class JB_Weapon : MonoBehaviour
{
    [HideInInspector]
    public ControlPC pc;
    [HideInInspector]
    public PUN_ControlPC PUNpc;
    public ParticleSystem onHitParticle;
    public ParticleSystem onHitParticlePlayer;

    public enum MagType
    {
        clip,
        energy,
    }

    public JB_GameManager.AllWeapons weaponType;

    // Magazine based weapon
    [Header("Magazine-based weapon")]
    public int clipSize;            // how many bullets before having to reload. 0 if infinite ammo
    public int ammoStock;
    [HideInInspector]
    public int currentAmmo;
    public int ammoUsagePrimary;        // how much ammo each fire uses
    public int ammoUsageSecondary;

    // Energy weapon
    [Header("Energy weapon")]
    public float timeToOverheatPrimary;    // how long until an energy weapon overheats. 0 if never overheats
    public float timeToOverheatSecondary;
    private float overheatTimer;
    [HideInInspector]
    public float overheatProgress;

    // Stats
    [Header("Weapon stats")]
    public int weaponDamagePrimary;
    public float fireRatePrimary;          // how many times per second can it fire. 0 if beam
    public int selfDamagePrimary = 0;   // if PC hits itself, what damage should be dealt
    public AudioSource firePrimarySFX;
    public int weaponDamageSecondary;
    public float fireRateSecondary;
    public int selfDamageSecondary = 0;  // if PC hits itself, what damage should be dealt
    public AudioSource fireSecondarySFX;
    public float timeToReload;
    [HideInInspector]
    public bool isReloading;
    public AudioSource reloadSFX;
    public AudioSource finishReloadSFX;
    [HideInInspector]
    public float fireTime;
    private bool canFire = true;
    [HideInInspector]
    public bool isFiring;
    [HideInInspector]
    public float fireDuration;

    private void Start()
    {
        pc = GetComponentInParent<ControlPC>();
        PUNpc = GetComponentInParent<PUN_ControlPC>();

        if (PUNpc == null)
        {
            Debug.LogError("JB_Weapon/ Start()/ No PUN_ControlPC found!!!!");
        }

        currentAmmo = clipSize;
    }

    public abstract void FireWeapon();
    public virtual void FireWeaponSecondary() { }
    public abstract void StoppedFiring();
    public virtual void StoppedFiringSecondary() { }

    public virtual void UpdateAmmoUI()
    {
        PUNpc.baseHud.SetAmmoCount(currentAmmo);
    }

    public virtual void CheckAmmo()
    {
        if (currentAmmo <= 0)
        {
            print("reached 0 ammo");
            StartCoroutine("StartReload");
        }
    }

    public void TryStartReload()
    {
        if (!isReloading && currentAmmo < clipSize)
        {
            print("player initiated reload");
            StartCoroutine("StartReload");
        }
    }

    public virtual IEnumerator StartReload()
    {
        if (!isReloading)
        {
            isReloading = true;
            if (currentAmmo != clipSize)    // if clip is not full
            {
                Instantiate(reloadSFX, transform.position, Quaternion.identity);
                // TODO reload animation
                yield return new WaitForSeconds(timeToReload);
                FinishReload();
            }
        }
    }

    public virtual void FinishReload()
    {
        Instantiate(finishReloadSFX, transform.position, Quaternion.identity);
        currentAmmo = clipSize;
        UpdateAmmoUI();
        isReloading = false;
        print("finished reload");
    }
}
