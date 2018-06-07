using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JB_CameraController : MonoBehaviour
{
    public Transform head;
    private ControlPC pc;
    private PUN_ControlPC PUNpc;
    private Vector3 nextPosition;

    void Start()
    {
        //pc = GetComponentInParent<ControlPC>();
        PUNpc = GetComponentInParent<PUN_ControlPC>();
        nextPosition.x = PUNpc.transform.position.x; //nextPosition.x = pc.transform.position.x;
        nextPosition.z = PUNpc.transform.position.z; //nextPosition.z = pc.transform.position.z;
        nextPosition.y = head.position.y;
        transform.position = nextPosition;
    }


    void Update()
    {
        nextPosition = PUNpc.transform.position; //nextPosition = pc.transform.position;
        nextPosition.y = head.position.y;
        transform.position = nextPosition;
    }
}
