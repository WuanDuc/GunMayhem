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
    //public static LobbyManage Instance;
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
    public List<Sprite> scenesList = new List<Sprite>();
    public List<string> sceneNameList = new List<string>();
    public GameObject leftMapButton, rightMapButton;
    public TMP_Text mapNameText;
    public Image mapImage;
    private int currentMapIndex = 0;
    //start game
    public GameObject playButton;
    public void Start()
    {
        SoundManager.PlaySound(SoundManager.Sound.Waitting);
        PhotonNetwork.AutomaticallySyncScene = true;
        
        PhotonNetwork.JoinLobby();
        UpdateMapUI();
        if (playerName == null)
        {
            //Debug.LogError("playerName is not assigned in the Inspector.");
        }
        else
        {
            Debug.Log("playerName is assigned successfully.");
            playerName.onValueChanged.AddListener(OnPlayerNameChanged);

            // Set initial nickname based on the current value in the playerName input field
            if (!string.IsNullOrEmpty(playerName.text))
            {
                PhotonNetwork.NickName = playerName.text;
                Debug.Log("PhotonNetwork.NickName set to: " + playerName.text);
            }
            else
            {
                //Debug.LogWarning("playerName is empty at start.");
            }
        }
        if (PhotonNetwork.InRoom)
        {

        }
    }
    private void OnDestroy()
    {
        if (playerName != null)
        {
            playerName.onValueChanged.RemoveListener(OnPlayerNameChanged);
        }
    }

    private void OnPlayerNameChanged(string newName)
    {
        PhotonNetwork.NickName = newName;
        Debug.Log("PhotonNetwork.NickName updated to: " + newName);
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
    //private void Awake()
    //{
    //    if (Instance == null)
    //    {
    //        Instance = this;
    //        DontDestroyOnLoad(gameObject);
    //    }
    //    else
    //    {
    //        Destroy(gameObject);
    //    }
    //}
    //create/join room
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
            //Debug.LogError("listLobbyPanel is null");
        }

        if (roomPanel)
        {
            roomPanel.SetActive(true);
        }
        else
        {
            //Debug.LogError("roomPanel is null");
        }

        if (playerRoomName)
        {
            playerRoomName.text = "Room name: " + PhotonNetwork.CurrentRoom.Name;
        }
        else
        {
           // Debug.LogError("playerRoomName is null");
        }

        UpdatePlayerList();
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Map"))
        {
            string mapName = PhotonNetwork.CurrentRoom.CustomProperties["Map"].ToString();
            currentMapIndex = scenesList.FindIndex(map => map.name == mapName);
            UpdateMapUI();
        }
        //UpdateMapUI();
    }
    public void GoBackToLobby()
    {
        PhotonNetwork.LeaveRoom();
    }
    public override void OnLeftRoom()
    {
        if (roomPanel)
        roomPanel.SetActive(false);
        if (listLobbyPanel)
        listLobbyPanel.SetActive(true);
    }
    //change map
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("Map"))
        {
            string mapName = propertiesThatChanged["Map"].ToString();
            currentMapIndex = scenesList.FindIndex(map => map.name == mapName);
            UpdateMapUI();
        }
    }
    void UpdateMapUI()
    {
        if (scenesList.Count > 0)
        {
            mapImage.sprite = scenesList[currentMapIndex];
            mapNameText.text = scenesList[currentMapIndex].name;
        }
    }
    public void OnClickLeftArrow()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            currentMapIndex--;
            if (currentMapIndex < 0)
            {
                currentMapIndex = scenesList.Count - 1;
            }
            UpdateMapUI();
            UpdateRoomProperties();
        }
    }
    public void OnClickRightArrow()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            currentMapIndex++;
            if (currentMapIndex >= scenesList.Count)
            {
                currentMapIndex = 0;
            }
            UpdateMapUI();
            UpdateRoomProperties();
        }
    }
    void UpdateRoomProperties()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
            customProperties["Map"] = scenesList[currentMapIndex].name;
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
        }
    }
    public void Quit()
    {
        SceneManager.LoadSceneAsync(0);
    }

    // Update is called once per frame
    private void Update()
    {
        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount >= 1)
            {
                playButton.SetActive(true);
                leftMapButton.SetActive(true);
                rightMapButton.SetActive(true);
            }
            else
            {
                playButton.SetActive(false);
                leftMapButton.SetActive(false);
                rightMapButton.SetActive(false);
            }
        }
    }
    //start game
    public void OnClickPlayButton()
    {
        PhotonNetwork.LoadLevel("Map03");
    }
    //players list
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
        if (list != null && contentObject!= null)
            foreach (RoomInfo item in list)
            {
                Debug.Log("RoomItem Prefab: " + roomItemPrefab);
              
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
        Debug.Log(roomName);
        Debug.Log("Attempting to join room: " + roomName);
        if (roomName != null)
        {
                PhotonNetwork.JoinRoom(roomName);
        }
        else
        {
            //Debug.LogError("Room name is null.");
        }
        //if (roomName != null)
        //{
        //    {
        //        if (playerName.text.Length >= 1)
        //        {
        //            PhotonNetwork.NickName = playerName.text;
        //        }
        //        else
        //        {
        //            Popup.SetActive(true);
        //            listLobbyPanel.SetActive(false);
        //            return;
        //        }

        //        PhotonNetwork.JoinRoom(roomName);
        //    }
        //}
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
