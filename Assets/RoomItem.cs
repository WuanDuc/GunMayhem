using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;

public class RoomItem : MonoBehaviour
{
    public TMP_Text roomName, hostName, memberNumber;
    LobbyManage manager;
    private void Start()
    {
        manager = FindObjectOfType<LobbyManage>();
        if (manager == null)
        {
            Debug.LogError("LobbyManage instance not found in the scene.");
        }
    }
    public void OnClickItem()
    {
        if (manager != null&& roomName.text != null)
        {
            manager.JoinRoom(roomName.text);
        }
    }
    public void SetRoomName(string roomName)
    {
        this.roomName.text = roomName;
    }
    public void SetHostName(string hostName)
    {
        this.hostName.text = hostName;
    }
    public void SetMemberNumber(string num)
    {
        this.memberNumber.text = num;
    }
}
