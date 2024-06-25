using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class LobbyManage : MonoBehaviourPunCallbacks
{
    //[Header("UI")] public Transform listLobbyParent;
    public GameObject listLobbyPanel;
    public GameObject roomPanel;
    public TMP_Text playerRoomName;
    public TMP_InputField roomInputField, playerName;
    public List<RoomInfo> cacheRoomInfo = new List<RoomInfo>();

    public RoomItem roomItemPrefab;
    List<RoomItem> roomItemsList = new List<RoomItem>();
    public Transform contentObject;
    // Start is called before the first frame update
    public float timeBetweenUpdates = 1.5f;
    float nextUpdateTime;
    public void Start()
    {
        PhotonNetwork.JoinLobby();
    }
    public void OnClickCreate()
    {
        if (roomInputField.text.Length >= 1)
        {
            PhotonNetwork.CreateRoom(roomInputField.text, new RoomOptions() { MaxPlayers = 4});
        }
        else 
        {
            PhotonNetwork.CreateRoom("Unname room", new RoomOptions() { MaxPlayers = 4 });
        }
    }
    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom called");

        if (listLobbyPanel)
        {
            listLobbyPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("listLobbyPanel is null");
        }

        if (roomPanel)
        {
            roomPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("roomPanel is null");
        }

        if (playerRoomName)
        {
            playerRoomName.text = "Room name: " + PhotonNetwork.CurrentRoom.Name;
        }
        else
        {
            Debug.LogError("playerRoomName is null");
        }
    }
    public void Quit()
    {
        SceneManager.LoadSceneAsync(0);
    }
    public void GoBackToLobby()
    {
        PhotonNetwork.LeaveRoom();
    }
    public override void OnLeftRoom()
    {
        roomPanel.SetActive(false);
        listLobbyPanel.SetActive(true);
    }
    // Update is called once per frame
    void Update()
    {

    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (Time.time >= nextUpdateTime)
        {
            UpdateRoomList(roomList);
            nextUpdateTime = Time.time + timeBetweenUpdates;
        }
    }
    void UpdateRoomList(List<RoomInfo> list)
    {
        foreach (RoomItem item in roomItemsList) 
        {
            Destroy(item.gameObject);
        }
        roomItemsList.Clear();

        foreach (RoomInfo item in list)
        {
            RoomItem newRoom = Instantiate(roomItemPrefab, contentObject);
            newRoom.SetRoomName(item.Name);
            newRoom.SetHostName("Guest");
            newRoom.SetMemberNumber(4);
            roomItemsList.Add(newRoom);
        }
    }
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }
}
