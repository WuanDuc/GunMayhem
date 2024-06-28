using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class GamePlay : MonoBehaviourPunCallbacks
{
    public GameObject selectedMapPrefab;
    public GameObject[] listMapPrefabs;
    public Transform mapParent;

    // Start is called before the first frame update
    void Start()
    {
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
            }
            else
            {
                Debug.LogError("Selected map prefab is null.");
            } 
        }
        else
        {
            Debug.LogWarning("Map property not found in custom properties.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
