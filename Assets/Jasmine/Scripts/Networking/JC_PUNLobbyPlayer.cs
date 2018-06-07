using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JC_PUNLobbyPlayer : Photon.PunBehaviour, IPunObservable
{
    [SerializeField] private int sendRate;
    [SerializeField] private int sendRateOnSerialize;

    // Use this for initialization
    void Start()
    {
        sendRate = 20;
        sendRateOnSerialize = 10;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}
