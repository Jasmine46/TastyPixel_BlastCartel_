using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class JB_Ammo : MonoBehaviour
{
    public Rigidbody rb;
    [HideInInspector]
    public int damage;
    public float maxLifetime;   // despawns after how long. 0 means infinite
    private float lifeTimer;
    public ParticleSystem onHitParticle;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

}
