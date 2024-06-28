using UnityEngine;

public class RandomBox : MonoBehaviour
{
    [SerializeField]
    private GameObject[] gunPrefabs;

    // Update is called once per frame
    void Update()
    {

    }
    public GameObject GetRamdomGun()
    {
        int randomIndex = Random.Range(0, gunPrefabs.Length);
        return gunPrefabs[randomIndex];
    }
   
}
