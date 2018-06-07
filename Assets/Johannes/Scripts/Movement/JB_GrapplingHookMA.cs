using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JB_GrapplingHookMA : JB_MovementAbility
{
    // Firing
    [Header("Firing")]
    public float maxTimeFired = 3;  // how long before it must be retracted
    private float firingTimer;
    public float firingForce;
    private bool hasFired;
    private Vector3 hookDirection;
    public JB_GrapplingHookHOOK hookPrefab;
    private JB_GrapplingHookHOOK firedHook;

    // Impacting
    private bool hasMadeContact;
    public float travelSpeedPC;
    private float appliedTravelSpeed;
    public float cancellationForce = 10;

    // Moving
    private Vector3 aimPosition;

    void Start()
    {

    }


    void Update()
    {
        if (hasFired)   // once it has been fired
        {
            if (hasMadeContact) // once it has hit something (e.g. terrain or player)
            {

            }

            else
            {
                firingTimer += Time.deltaTime;
                if (firingTimer >= maxTimeFired)
                {
                    CancelAbility();
                }
            }
        }
    }

    void FixedUpdate()
    {
        
    }

    public override void UseAbility(Vector3 direction)
    {
        if (!hasFired && Time.time >= cooldownTime)
        {
            if (firedHook) Destroy(firedHook.gameObject);
            firedHook = Instantiate(hookPrefab, PUNpc.cam.transform.position /*pc.cam.transform.position*/ + direction, Quaternion.identity);
            firedHook.sender = this;
            firedHook.rb.velocity = direction * firingForce;
            firedHook.direction = direction;
            hasMadeContact = false;
            hasFired = true;
        }
        else if (hasFired)
        {
            CancelAbility();
        }
    }

    public void HookImpactTerrain(Vector3 normal)
    {
        //pc.movedByAbility = true;
        PUNpc.movedByAbility = true;
        hasMadeContact = true;
        cooldownTime = Time.time + abilityCooldown;
        aimPosition = firedHook.transform.position + normal;
        StopCoroutine("MoveToHook");
        StartCoroutine("MoveToHook");
    }

    IEnumerator MoveToHook()
    {
        appliedTravelSpeed = travelSpeedPC / 2;
        Vector3 heading = aimPosition - PUNpc.transform.position;
        float distance = heading.magnitude;
        hookDirection = heading / distance;

        while (hasFired && distance > 2)
        {
            PUNpc.cc.Move(hookDirection * appliedTravelSpeed * Time.deltaTime);                // move PC towards hook
            if (firedHook) heading = aimPosition - PUNpc.transform.position;  
            distance = heading.magnitude;
            hookDirection = heading / distance;
            if (appliedTravelSpeed < travelSpeedPC) appliedTravelSpeed += Time.deltaTime;
            yield return null;
        }

        CancelAbility();

        //pc.movementModifiers.Add(new MovementMod(hookDirection + Vector3.up * cancellationForce, .5f, true, false, true));
        //pc.appliedGravity = 0;
        //pc.movedByAbility = inMotion = false;
    }

    public override void CancelAbility()
    {
        if (hasFired && hasMadeContact)
        {
            StopCoroutine("MoveToHook");
            PUNpc.movementModifiers.Add(new MovementMods(hookDirection * 2 + Vector3.up * cancellationForce, Time.time, Time.time + 1, true, true, false));
            PUNpc.appliedGravity = 0;
            PUNpc.movedByAbility = hasMadeContact = false;
            firingTimer = 0;
            cooldownTime = Time.time + abilityCooldown;
        }
        else
        {
            cooldownTime = firingTimer = 0;
        }

        hasFired = false;
        if (firedHook) Destroy(firedHook.gameObject);
    }
}
