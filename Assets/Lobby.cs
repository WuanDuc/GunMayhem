using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviour
{
    [Header("UI")] public Transform listLobbyParent;
    public GameObject listLobbyParentPrefab;
    private List<RoomInfo> cacheRoomInfo = new List<RoomInfo>();
    // Start is called before the first frame update
    IEnumerable Start()
    {
        //Precoustion
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.Disconnect();
        }
        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);
        PhotonNetwork.ConnectUsingSettings();
    }
    public void Quit()
    {
        SceneManager.LoadSceneAsync(0);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
