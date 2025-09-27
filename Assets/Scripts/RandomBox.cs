using Photon.Pun;
using System.Collections;
using UnityEngine;

public class RandomBox : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject[] gunPrefabs;
    private PhotonView photonView;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    public GameObject GetRandomGun()
    {
        int randomIndex = Random.Range(0, gunPrefabs.Length);
        return gunPrefabs[randomIndex];
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Chỉ gửi request lên Master Client
            PhotonView playerPV = collision.GetComponent<PhotonView>();
            if (playerPV != null)
            {
                photonView.RPC("RequestEquipWeapon", RpcTarget.MasterClient, playerPV.ViewID);
            }
        }
    }

    [PunRPC]
    public void RequestEquipWeapon(int playerViewID)
    {
        // Chỉ Master Client xử lý
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonView playerPV = PhotonView.Find(playerViewID);
        if (playerPV != null)
        {
            // Get random weapon
            GameObject randomWeapon = GetRandomGun();
            
            // Spawn weapon trên network
            GameObject spawnedWeapon = PhotonNetwork.Instantiate(randomWeapon.name, transform.position, transform.rotation);
            
            // Thông báo cho player equip weapon
            photonView.RPC("EquipWeaponToPlayer", RpcTarget.All, playerViewID, spawnedWeapon.GetComponent<PhotonView>().ViewID);
            
            // Destroy box
            PhotonNetwork.Destroy(gameObject);
        }
    }

    [PunRPC]
    void EquipWeaponToPlayer(int playerViewID, int weaponViewID)
    {
        PhotonView playerPV = PhotonView.Find(playerViewID);
        PhotonView weaponPV = PhotonView.Find(weaponViewID);
        
        if (playerPV != null && weaponPV != null)
        {
            WeaponHandler weaponHandler = playerPV.GetComponent<WeaponHandler>();
            if (weaponHandler != null)
            {
                weaponHandler.EquipWeapon(weaponPV.gameObject);
            }
        }
    }
}
