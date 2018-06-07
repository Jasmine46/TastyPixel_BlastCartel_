using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class JB_MovementAbility : MonoBehaviour
{
    [HideInInspector]
    public ControlPC pc;
    [HideInInspector]
    public PUN_ControlPC PUNpc;

    public float abilityCooldown;
    [HideInInspector]
    public float cooldownTime;
    [HideInInspector]
    public bool inMotion;

    public abstract void UseAbility(Vector3 direction);

    public abstract void CancelAbility();
}
