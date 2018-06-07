using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JB_Weapon_NoobTube : JB_Weapon
{
    public float fireForce;
    public float fireDegreesAboveReticle = 5;
    

    void Update()
    {

    }

    public override void FireWeapon()
    {
        if (!isReloading && Time.time >= fireTime && currentAmmo > 0)
        {
            fireTime = Time.time + 1 / fireRatePrimary;
            Vector3 fireTraj = (Quaternion.AngleAxis(fireDegreesAboveReticle, PUNpc.cam.transform.TransformDirection(Vector3.left)) * PUNpc.cam.transform.TransformDirection(Vector3.forward)) * fireForce;
            PUNpc.FireAmmo(weaponType, JB_GameManager.AttackTypes.Primary, fireTraj, weaponDamagePrimary);
        }
    }

    public override void FireWeaponSecondary()
    {
        if (!isReloading && Time.time >= fireTime && currentAmmo > 0)
        {
            fireTime = Time.time + 1 / fireRatePrimary;
            Vector3 fireTraj = (Quaternion.AngleAxis(fireDegreesAboveReticle, PUNpc.cam.transform.TransformDirection(Vector3.left)) * PUNpc.cam.transform.TransformDirection(Vector3.forward)) * fireForce;
            PUNpc.FireAmmo(weaponType, JB_GameManager.AttackTypes.Secondary, fireTraj, weaponDamageSecondary);
        }
    }

    public override void StoppedFiring()
    {}

    public override IEnumerator StartReload()
    {
        if (currentAmmo != clipSize)    // if clip is not full
        {
            // TODO reload animation
            yield return new WaitForSeconds(timeToReload);
            FinishReload();
        }
    }
}
