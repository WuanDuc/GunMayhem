using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    public Transform[] spawnerPoints;
    public GameObject[] playerPrefabs;

    // Start is called before the first frame update
    void Start()
    {
        int randomNumber = Random.Range(0, spawnerPoints.Length);
        Transform spawnPoint = spawnerPoints[randomNumber];
        Vector3 spawnPosition = new Vector3(spawnPoint.position.x, spawnPoint.position.y, 1);
        int playerSkin = (int)PhotonNetwork.LocalPlayer.CustomProperties["playerAvatar"];
        GameObject playerToSpawn = playerPrefabs[playerSkin];
        Debug.Log(spawnPosition);
        GameObject spawnedPlayer = PhotonNetwork.Instantiate(playerToSpawn.name, spawnPosition, Quaternion.identity);

        // Set camera target to the spawned player if it's the local player
        if (spawnedPlayer.GetComponent<PhotonView>().IsMine)
        {
            Camera.main.GetComponent<CameraFollow>().SetTarget(spawnedPlayer.transform);
        }
    }
}
