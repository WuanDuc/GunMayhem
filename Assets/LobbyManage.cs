using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SearchService;

public class LobbyManage : MonoBehaviourPunCallbacks
{
    //[Header("UI")] public Transform listLobbyParent;
    public GameObject listLobbyPanel;
    public GameObject roomPanel;
    public TMP_Text playerRoomName;
    public TMP_InputField roomInputField, playerName;
    public List<RoomInfo> cacheRoomInfo = new List<RoomInfo>();
    public GameObject Popup;
    public RoomItem roomItemPrefab;
    List<RoomItem> roomItemsList = new List<RoomItem>();
    public Transform contentObject;
    // Start is called before the first frame update
    public float timeBetweenUpdates = 1.5f;
    float nextUpdateTime;

    //players
    public List<RoomPlayer> roomPlayersList = new List<RoomPlayer>();
    public RoomPlayer roomPlayerPrefab;
    public Transform roomPlayerParent;
    //maps
    public List<Image> scenesList = new List<Image>();
    public List<string> sceneNameList = new List<string>();

    public GameObject playButton;
    public void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.JoinLobby();
    }
    public void CloseModal()
    {
        if (Popup != null)
        {
            Popup.SetActive(false);
        }
        if (listLobbyPanel != null)
        { listLobbyPanel.SetActive(true); }
    }
    public void OnClickCreate()
    {
        if (playerName.text.Length >= 1)
        {
            PhotonNetwork.NickName = playerName.text;
        }
        else
        {
            Popup.SetActive(true);
            listLobbyPanel.SetActive(false);
            return;
        }
        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
        customProperties.Add("HostName", PhotonNetwork.NickName);

        if (roomInputField.text.Length >= 1)
        {

            PhotonNetwork.CreateRoom(roomInputField.text, new RoomOptions() { 
                MaxPlayers = 4,
                BroadcastPropsChangeToAll = true,
                CustomRoomProperties = customProperties,
                CustomRoomPropertiesForLobby = new string[] {"HostName"}
            });
        }
        else 
        {
            PhotonNetwork.CreateRoom("Unname room", new RoomOptions() { 
                MaxPlayers = 4,
                BroadcastPropsChangeToAll = true,
                CustomRoomProperties = customProperties,
                CustomRoomPropertiesForLobby = new string[] { "HostName" }
            });
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
        UpdatePlayerList();
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
    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount >= 1)
        {
            playButton.SetActive(true);
        }
        else
        {
            playButton.SetActive(false);
        }
    }
    public void OnClickPlayButton()
    {
        PhotonNetwork.LoadLevel("Game");
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
            if (item.CustomProperties.ContainsKey("HostName"))
            {
                string hostName = (string)item.CustomProperties["HostName"];
                newRoom.SetHostName(hostName);
            }
            else
            {
                newRoom.SetHostName("Unknown");
            }
            newRoom.SetMemberNumber(item.PlayerCount + "/4");
            roomItemsList.Add(newRoom);
        }
    }
    public void JoinRoom(string roomName)
    {
        if (playerName.text.Length >= 1)
        {
            PhotonNetwork.NickName = playerName.text;
        }
        else
        {
            Popup.SetActive(true);
            listLobbyPanel.SetActive(false);
            return;
        }

        PhotonNetwork.JoinRoom(roomName);
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }
    void UpdatePlayerList()
    {
        foreach (RoomPlayer item in roomPlayersList)
        {
            Destroy(item.gameObject);
        }
        roomPlayersList.Clear();

        if (PhotonNetwork.CurrentRoom == null)
        {
            return;
        }else
        {
            Debug.Log(PhotonNetwork.CurrentRoom.ToString());
        }
        foreach (KeyValuePair<int,Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            if (roomPlayerPrefab && roomPlayerParent)
            {
                RoomPlayer newPlayerItem = Instantiate(roomPlayerPrefab, roomPlayerParent);
                newPlayerItem.SetPlayerInfo(player.Value);
                if (player.Value == PhotonNetwork.LocalPlayer)
                {
                    newPlayerItem.ApplyLocalChanges();
                }
                roomPlayersList.Add(newPlayerItem);
            }
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }
    public override void OnPlayerLeftRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }

}
