using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//[NetworkSettings(channel = 1, sendInterval = 0.0333f)]
public class JB_AmmoSync : Photon.PunBehaviour, IPunObservable//NetworkBehaviour
{
    private Vector3 nextPos;
    private float netStep = 0;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.isMine)//(isLocalPlayer)
        {
            //netStep += Time.deltaTime;
            //if (netStep >= GetNetworkSendInterval())
            //{
            //    netStep = 0;
            //    CmdUpdateTransform(transform.position);
            //}
        }

        else
        {
            LerpTransform();
        }
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }

    //[Command]
    void /*Cmd*/UpdateTransform(Vector3 _nextPos)
    {
        /*Rpc*/UpdateClientTransform(_nextPos);
    }

    //[ClientRpc]
    void /*Rpc*/UpdateClientTransform(Vector3 _nextPos)
    {
        nextPos = _nextPos;
    }

    void LerpTransform()
    {
        transform.position = Vector3.Lerp(transform.position, nextPos, .5f);
    }
}
