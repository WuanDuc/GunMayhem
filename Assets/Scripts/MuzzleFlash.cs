using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class MuzzleFlash : MonoBehaviour
{

    public void Destroy()
    {
        PhotonNetwork.Destroy(gameObject);
    }
}
