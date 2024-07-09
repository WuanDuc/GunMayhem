using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
public class GamePlay : MonoBehaviourPunCallbacks
{
    public GameObject selectedMapPrefab;
    public GameObject[] listMapPrefabs;
    public Transform mapParent;

    //start game
    public Transform startGamePanel;
    public TMP_Text startTimerText; // show the start timer
    public TMP_Text matchTimerText; // show the match timer
    public GameObject winLosePanel; // Panel to show win/lose message
    public TMP_Text winLoseText; // Text to show win/lose message
    public GameObject pausePanel;

    private float matchDuration = 90f;
    private float startTime = 3.0f;

    public GameObject playerPanelPrefab; // Reference to the player panel prefab
    public Transform playerPanelContainer; // Reference to the container for player panels

    private List<GameObject> playerPanels = new List<GameObject>();

    PlayerMovement Player;
    void Start()
    {
        SoundManager.PlaySound(SoundManager.Sound.Theme);   
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Map"))
        {
            string mapName = PhotonNetwork.CurrentRoom.CustomProperties["Map"].ToString();
            switch (mapName)
            {
                case "map_01":
                    selectedMapPrefab = listMapPrefabs[0];
                    break;
                case "map_02":
                    selectedMapPrefab = listMapPrefabs[1];
                    break;
                case "map_03":
                    selectedMapPrefab = listMapPrefabs[2];
                    break;
                case "map_04":
                    selectedMapPrefab = listMapPrefabs[3];
                    break;
                case "map_05":
                    selectedMapPrefab = listMapPrefabs[4]; 
                    break;
                case "map_06":
                    selectedMapPrefab = listMapPrefabs[5];
                    break;
                case "map_07":
                    selectedMapPrefab = listMapPrefabs[6];
                    break;
                case "map_08":
                    selectedMapPrefab = listMapPrefabs[7];
                    break;
                case "map_09":
                    selectedMapPrefab = listMapPrefabs[8];
                    break;
                case "map_10":
                    selectedMapPrefab = listMapPrefabs[9];
                    break;
                default:
                    break;
            }
            if (selectedMapPrefab != null)
            {
                GameObject instantiatedMap = Instantiate(selectedMapPrefab, mapParent);
                Debug.Log("Map instantiated: " + instantiatedMap.name);

                //SpawnRandomBox spawnRandomBox = instantiatedMap.GetComponentInChildren<SpawnRandomBox>();
                //if (spawnRandomBox != null)
                //{
                //    float randomTime = Random.Range(10f, 20f);

                //    spawnRandomBox.setTimeSpawm(randomTime); // Set initial spawn time to 3 seconds

                //}
            }
            else
            {
                Debug.LogError("Selected map prefab is null.");
                return;
            } 
        }
        else
        {
            Debug.LogWarning("Map property not found in custom properties.");
            return;
        }
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();
            playerProperties["deaths"] = 0;
            player.SetCustomProperties(playerProperties);
        }
        UpdatePlayerPanels();
        StartCoroutine(StartGameCountdown());
        
    }
    IEnumerator StartGameCountdown()
    {
        while (startTime > 0)
        {
            startTimerText.text = startTime.ToString("F0");
            yield return new WaitForSeconds(1f);
            startTime--;
        }
        //Player.Activate();
        startTimerText.text = "GO!";
        yield return new WaitForSeconds(1f);
        startTimerText.gameObject.SetActive(false);
        matchTimerText.gameObject.SetActive(true);
        
        // start the match timer
        StartCoroutine(MatchTimer());

        //GameObject instantiatedMap = mapParent.GetChild(0).gameObject;
        //SpawnRandomBox spawnRandomBox = instantiatedMap.GetComponentInChildren<SpawnRandomBox>();
        //if (spawnRandomBox != null)
        //{
        //    float randomTime = Random.Range(10f, 20f);
        //    spawnRandomBox.setTimeSpawm(randomTime);
        //}
    }
    IEnumerator MatchTimer()
    {
        
        float timeRemaining = matchDuration;
        while (timeRemaining > 0)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            matchTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            yield return new WaitForSeconds(1f);
            timeRemaining--;
        }
        matchTimerText.text = "00:00";

        // Show win/lose panel
        ShowWinLosePanel();
    }

    void ShowWinLosePanel()
    {
        winLosePanel.SetActive(true);

        //  winner
        Player winner = null;
        int highestPoints = -1;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            int playerPoints = (int)player.CustomProperties["deaths"];
            if (playerPoints < highestPoints)
            {
                highestPoints = playerPoints;
                winner = player;
            }
        }

        if (winner != null)
        {
            winLoseText.text = "Winner: " + winner.NickName + " with " + highestPoints + " deaths!";
        }
        else
        {
            winLoseText.text = "Game Over. No winner!";
        }
        StartCoroutine(ReturnToRoomAfterDelay(5f));
    }
    IEnumerator ReturnToRoomAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("SelectScreen"); 
    }
    void UpdatePlayerPanels()
    {
        // Clear existing player panels
        if (playerPanels != null)
        {
            foreach (GameObject panel in playerPanels)
            {
                Destroy(panel);
            }
            playerPanels.Clear();
        }
        if (playerPanelContainer != null)
        // Create a panel for each player in the room
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject panel = Instantiate(playerPanelPrefab, playerPanelContainer);
            TMP_Text playerNameText = panel.transform.Find("PlayerNameText").GetComponent<TMP_Text>();
            TMP_Text playerDeathsText = panel.transform.Find("PlayerDeathsText").GetComponent<TMP_Text>();

            playerNameText.text = player.NickName;
            if (player.CustomProperties.ContainsKey("deaths"))
            {
                playerDeathsText.text = "Deaths: " + player.CustomProperties["deaths"].ToString();
            }
            else
            {
                playerDeathsText.text = "Deaths: 0";
            }
            playerPanels.Add(panel);
        }
    }
    // Update is called once per frame
    void Update()
    {
        UpdatePlayerPanels();
    }
    public void PauseGame()
    {
        pausePanel.SetActive(true);
    }
    public void UnPauseGame()
    {
        pausePanel.SetActive(false);
    }
    public void LeftRoom()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("SelectScreen");

    }
}
