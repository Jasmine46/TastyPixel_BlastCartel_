using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JC_PUNSpawner : MonoBehaviour
{ 
    [SerializeField] private bool isFree = false;

    // Add a Trigger that sends an RPC call 
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.GetComponent<PhotonView>())
        {
            isFree = false;
        }

        else
        {
            isFree = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<PhotonView>())
        {
            isFree = true;
        }

        else
        {
            isFree = false;
        }
    }

    public void SetIsFree(bool vIsFree)
    {
        if (vIsFree != isFree)
        {
            isFree = vIsFree;
        }
    }

    public bool GetIsFree()
    {
        return isFree;
    }
}
