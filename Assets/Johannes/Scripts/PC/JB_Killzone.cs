using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JB_Killzone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        ControlPC pc = other.GetComponent<ControlPC>();
        if (pc)
        {
            pc.CmdEnterKillzone();
        }
    }
}
