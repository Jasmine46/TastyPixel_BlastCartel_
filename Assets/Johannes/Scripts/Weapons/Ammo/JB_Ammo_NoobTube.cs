﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JB_Ammo_NoobTube : JB_Ammo
{
    public float explosionRadius;
    public float pushForce = 20;
    private List<ControlPC> playersHit = new List<ControlPC>();
    private List<PUN_ControlPC> PUNplayersHit = new List<PUN_ControlPC>();
    private float lifetimer = 0;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        lifetimer += Time.deltaTime;
        if (lifetimer > 3)
        {
            rb.drag = 0;
        }

        else
        {
            //print(rb.velocity);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, explosionRadius, collision.contacts[0].normal, 1, JB_GameManager.gm.weaponLayer);
        
        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                HitboxLink hbl = hits[0].collider.GetComponent<HitboxLink>();
                if (hbl)
                {
                    // has this PC been accounted for already
                    if (!HasPlayerBeenHit(hbl.PUNpc /*hbl.pc*/))
                    {
                        // if no terrain between explosion and player hitbox link
                        if (!Physics.Linecast(transform.position, hbl.transform.position, JB_GameManager.gm.terrainLayer))
                        {
                            // add pc to players hit list, explode on pc
                            PUNplayersHit.Add(hbl.PUNpc); //playersHit.Add(hbl.pc);
                        }
                    }
                }
            }

            foreach (PUN_ControlPC _pc in PUNplayersHit /*ControlPC _pc in playersHit*/)
            {
                //_pc.CmdTakeDamage(damage);
                Vector3 dir = (_pc.transform.position + Vector3.up) - transform.position;
                dir.Normalize();
                _pc.movementModifiers.Add(new MovementMods(dir * pushForce, Time.time, Time.time + .5f, true, false, true));
            }
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        Instantiate(onHitParticle, transform.position, Quaternion.identity);
    }

    private bool HasPlayerBeenHit(PUN_ControlPC _pc /*ControlPC _pc*/)
    {
        foreach (PUN_ControlPC item in PUNplayersHit /*ControlPC item in playersHit*/)
        {
            if (item == _pc)
            {
                return true;
            }
        }

        return false;
    }
}
