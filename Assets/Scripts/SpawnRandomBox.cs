using Photon.Pun;
using UnityEngine;

public class SpawnRandomBox : MonoBehaviour
{
    [SerializeField]
    private float timeBetweenSpawn = 5f;

    [SerializeField]
    private GameObject boxPrefab;

    [SerializeField]
    private Transform spawnPlace;

    private bool isSpawning = false;

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartSpawning(15f);
        }
    }

    public void setTimeSpawm(float timeBetweenSpawn)
    {
        this.timeBetweenSpawn = timeBetweenSpawn;
        if (isSpawning && PhotonNetwork.IsMasterClient)
        {
            CancelInvoke("SpawnBox");
            InvokeRepeating("SpawnBox", timeBetweenSpawn, timeBetweenSpawn);
        }
    }

    private void StartSpawning(float initialSpawnTime)
    {
        isSpawning = true;

        InvokeRepeating("SpawnBox", initialSpawnTime, initialSpawnTime);
    }

    private void SpawnBox()
    {
        if (PhotonNetwork.IsConnected) {
            if (PhotonNetwork.IsMasterClient)
            {
                Vector3 spawnPos = spawnPlace != null ? spawnPlace.position : transform.position;
                spawnPos.z = 1;
                GameObject box = PhotonNetwork.Instantiate(boxPrefab.name, spawnPos, Quaternion.identity);
                Vector3 boxPosition = box.transform.localPosition;
                boxPosition.z = 1f;
                box.transform.localPosition = boxPosition;
            }
        }
        else
        {
            Vector3 spawnPos = spawnPlace != null ? spawnPlace.position : transform.position;
            spawnPos.z = 1;
            GameObject box = Instantiate(boxPrefab, spawnPos, Quaternion.identity);
            Vector3 boxPosition = box.transform.localPosition;
            boxPosition.z = 1f;
            box.transform.localPosition = boxPosition;
        }
    }
    private void Update()
    {

    }
}
