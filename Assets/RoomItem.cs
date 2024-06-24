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
    }
    public void OnClickItem()
    {
        manager.JoinRoom(roomName.text);
    }
    public void SetRoomName(string roomName)
    {
        this.roomName.text = roomName;
    }
    public void SetHostName(string hostName)
    {
        this.hostName.text = hostName;
    }
    public void SetMemberNumber(int num)
    {
        this.memberNumber.text = num.ToString();
    }
}
