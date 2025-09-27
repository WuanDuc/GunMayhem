using Photon.Pun;
using System.Collections;
using UnityEngine;

public class RandomBox : MonoBehaviour
{
    [SerializeField]
    private GameObject[] gunPrefabs;
    private PhotonView photonView;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public GameObject GetRamdomGun()
    {
        int randomIndex = Random.Range(0, gunPrefabs.Length);
        return gunPrefabs[randomIndex];
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // OLD CODE - commented out
            /*
            if (photonView.IsMine)
            {
                GameObject randomGun = GetRamdomGun();
                collision.GetComponent<WeaponHandler>().EquipWeapon(randomGun);
                DestroyBox();
            }
            else
            {
                photonView.RPC("RequestDestroyBox", RpcTarget.Others, photonView.ViewID);
            }
            */

            // NEW: Request weapon from Master Client
            PhotonView playerPhotonView = collision.GetComponent<PhotonView>();
            if (playerPhotonView != null && playerPhotonView.IsMine)
            {
                Debug.Log($"Player {playerPhotonView.Owner.NickName} requesting weapon from box");

                // Send request to Master Client
                photonView.RPC("RequestWeaponFromBox", RpcTarget.MasterClient,
                    playerPhotonView.ViewID, photonView.ViewID);
            }
        }
    }

    // NEW: Master Client handles weapon distribution
    [PunRPC]
    public void RequestWeaponFromBox(int playerViewID, int boxViewID)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonView targetPlayer = PhotonView.Find(playerViewID);
        PhotonView targetBox = PhotonView.Find(boxViewID);

        if (targetPlayer == null || targetBox == null) return;

        Debug.Log($"Master Client processing weapon request from {targetPlayer.Owner.NickName}");

        // Get random weapon
        GameObject randomWeapon = GetRamdomGun();

        // Spawn weapon for the player
        Vector3 weaponSpawnPos = targetPlayer.transform.Find("WeaponManager").position;
        GameObject spawnedWeapon = PhotonNetwork.Instantiate(randomWeapon.name, weaponSpawnPos, Quaternion.identity);

        // Notify player to equip the weapon
        targetPlayer.RPC("EquipNetworkWeapon", targetPlayer.Owner, spawnedWeapon.GetComponent<PhotonView>().ViewID);

        // Destroy the box
        targetBox.RPC("DestroyBox", RpcTarget.All);
    }

    private void DestroyedBox()
    {
        Debug.Log("Destroying random box");
        if (PhotonNetwork.IsConnected)
        {
            if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [PunRPC]
    public void DestroyBox()
    {
        DestroyedBox();
    }

    // OLD CODE - keep for compatibility but not used in new system
    [PunRPC]
    public void RequestDestroyBox(int viewID)
    {
        PhotonView targetView = PhotonView.Find(viewID);
        if (targetView != null && targetView.IsMine)
        {
            targetView.GetComponent<RandomBox>().DestroyBox();
        }
        else
        {
            Debug.LogWarning("Requested PhotonView not found or not owned by this client.");
        }
    }
}
