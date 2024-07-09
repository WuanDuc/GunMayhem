using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using UnityEngine.UI;
public class MainMenu : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        SoundManager.Initialize();
        SoundManager.PlaySound(SoundManager.Sound.Lobby);
    }
    public void PlayGame()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server. Loading game scene...");
        SceneManager.LoadSceneAsync(1);
    }

    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        Debug.LogError($"Disconnected from Photon with reason: {cause}");
        // Optionally, handle the disconnection (e.g., show a message to the player)
    }
    public void OptionGame()
    {
        SceneManager.LoadSceneAsync(2);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
