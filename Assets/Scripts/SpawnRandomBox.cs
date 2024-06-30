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
        StartSpawning(15f); 
    }

    public void setTimeSpawm(float timeBetweenSpawn)
    {
        this.timeBetweenSpawn = timeBetweenSpawn;
        if (isSpawning)
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
        Vector3 spawnPos = spawnPlace != null ? spawnPlace.position : transform.position;
        spawnPos.z = 1;
        GameObject box = Instantiate(boxPrefab, spawnPos, Quaternion.identity);
        Vector3 boxPosition = box.transform.localPosition;
        boxPosition.z = 1f;
        box.transform.localPosition = boxPosition;
    }
    private void Update()
    {

    }
}
