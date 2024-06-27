using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class RoomPlayer : MonoBehaviourPunCallbacks
{
    public TMP_Text playerName;
    public Image backgroundImage;
    public Color highlightcolor;
    public GameObject leftArrow, rightArrow;
    ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();

    public Image playerAvatar;
    public Sprite[] Avatars;

    Player player;
    public void SetPlayerInfo(Player _player)
    {
        playerName.text = _player.NickName;
        player = _player;
        UpdatePlayerItem(player);
    }
    private void Awake()
    {
        backgroundImage = GetComponent<Image>();

        //playerProperties["name"] = ""
    }
    // Start is called before the first frame update
    private void Start()
    {
        backgroundImage = GetComponent<Image>();
        playerProperties["playerAvatar"] = 0;
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
    }
    public void ApplyLocalChanges()
    {
        if (player != null)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = highlightcolor;
            }
            //playerName.color = highlightcolor;
            leftArrow.SetActive(true);
            rightArrow.SetActive(true);

        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnClickLeftArrow()
    {
        if ((int)playerProperties["playerAvatar"]==0)
        {
            playerProperties["playerAvatar"] = Avatars.Length - 1;
        }
        else
        {
            playerProperties["playerAvatar"] = (int)playerProperties["playerAvatar"] - 1;
        }
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
    }
    public void OnClickRightArrow()
    {
        if ((int)playerProperties["playerAvatar"] == Avatars.Length - 1)
        {
            playerProperties["playerAvatar"] = 0;
        }
        else
        {
            playerProperties["playerAvatar"] = (int)playerProperties["playerAvatar"] + 1;
        }
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (player == targetPlayer)
        {
            UpdatePlayerItem(targetPlayer);
        }
    }
    void UpdatePlayerItem(Player targetPlayer)
    {
        if (targetPlayer.CustomProperties.ContainsKey("playerAvatar"))
        {
            playerAvatar.sprite = Avatars[(int)targetPlayer.CustomProperties["playerAvatar"]];
            playerProperties["playerAvatar"] = (int)targetPlayer.CustomProperties["playerAvatar"];
        }
        else
        {
            playerProperties["playerAvatar"] = 0;
        }
    }
}
