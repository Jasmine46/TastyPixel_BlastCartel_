using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JB_DestroyIn : MonoBehaviour
{
    public float destroyIn;

    void Start()
    {
        destroyIn = Time.time + destroyIn;
    }
    
    void Update()
    {
        if (Time.time >= destroyIn)
        {
            Destroy(this.gameObject);
        }
    }
}
