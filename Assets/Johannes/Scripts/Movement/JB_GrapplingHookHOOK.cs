using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JB_GrapplingHookHOOK : MonoBehaviour
{
    public Rigidbody rb;
    private LineRenderer lr;
    public JB_GrapplingHookMA sender;
    public LayerMask layersToHit;

    [HideInInspector]
    public Vector3 direction;
    private bool destinationFound;
    private int terrainLayermask = 1 << 8;

    private void Start()
    {
        lr = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (!sender)
        {
            Destroy(this.gameObject);
        }
    }

    void FixedUpdate()
    {
        if (!destinationFound)
        {
            CheckForObjectAhead();
        }

        CheckLineOfSight();
    }

    void CheckForObjectAhead()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, direction, out hit, 1, layersToHit))  // if it's about to hit terrain or a player
        {
            rb.velocity = Vector3.zero;
            transform.position = hit.point + hit.normal / 10;
            destinationFound = true;
            transform.parent = hit.collider.transform;
            if (sender) sender.HookImpactTerrain(hit.normal);
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.gameObject.layer == 8)    // if it hits terrain
    //    {
    //        destinationFound = true;
    //        rb.velocity = Vector3.zero;
    //        transform.parent = other.transform;
    //        if (sender) sender.HookImpactTerrain(other.);
    //    }
    //}

    private void CheckLineOfSight()
    {
        if (sender && sender.PUNpc /*sender.pc*/)
        {
            // if a piece of terrain is between the hook and the pc
            if (Physics.Linecast(transform.position, sender.PUNpc.cam.transform.position /*sender.pc.cam.transform.position*/, terrainLayermask, QueryTriggerInteraction.Ignore))
            {
                sender.CancelAbility();
            }

            else
            {
                lr.SetPosition(0, transform.position);
                lr.SetPosition(1, sender.PUNpc.transform.position /*sender.pc.transform.position*/ + Vector3.up / 20);
            }
        }
    }
}
