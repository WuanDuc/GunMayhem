using UnityEngine;

public class SpawnRandomBox : MonoBehaviour
{
    [SerializeField]
    private float timeBetweenSpawn = 5f;

    [SerializeField]
    private GameObject boxPrefab;

    [SerializeField]
    private Transform spawnPlace;
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("SpawnBox", timeBetweenSpawn,timeBetweenSpawn);
    }

    private void SpawnBox()
    {
        GameObject box = Instantiate(boxPrefab, spawnPlace);
        box.transform.localPosition= Vector3.zero;
    }
}
