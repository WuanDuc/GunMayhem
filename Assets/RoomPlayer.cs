using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class RoomPlayer : MonoBehaviour
{
    public TMP_Text playerName;
    public Image backgroundImage;
    public Color highlightcolor;
    public GameObject leftArrow, rightArrow;
    public void SetPlayerInfo(Player _player)
    {
        playerName.text = _player.NickName;
    }
    // Start is called before the first frame update
    private void Start()
    {
        backgroundImage = GetComponent<Image>();
    }
    public void ApplyLocalChanges()
    {
        backgroundImage.color = highlightcolor;
        playerName.color = highlightcolor;
        leftArrow.SetActive(true);
        rightArrow.SetActive(true);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
