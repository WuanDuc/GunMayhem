using Photon.Pun;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        // Initialize bullet pool nếu là Master Client
        if (PhotonNetwork.IsMasterClient && BulletPool.Instance == null)
        {
            // Tạo BulletPool GameObject
            GameObject poolGO = new GameObject("BulletPool");
            poolGO.AddComponent<BulletPool>();
        }
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        Debug.Log($"Master Client switched to: {newMasterClient.NickName}");

        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            Debug.Log("I am the new Master Client - taking control of game logic");
            TakeControlOfGameObjects();
        }
    }

    private void TakeControlOfGameObjects()
    {
        // Take control of bullets
        Bullet[] bullets = FindObjectsOfType<Bullet>();
        foreach (var bullet in bullets)
        {
            if (!bullet.photonView.IsMine)
            {
                bullet.photonView.RequestOwnership();
            }
        }

        // Take control of random boxes
        RandomBox[] boxes = FindObjectsOfType<RandomBox>();
        foreach (var box in boxes)
        {
            if (!box.GetComponent<PhotonView>().IsMine)
            {
                box.GetComponent<PhotonView>().RequestOwnership();
            }
        }

        // Take control of spawn boxes
        SpawnRandomBox[] spawnBoxes = FindObjectsOfType<SpawnRandomBox>();
        foreach (var spawnBox in spawnBoxes)
        {
            if (!spawnBox.GetComponent<PhotonView>().IsMine)
            {
                spawnBox.GetComponent<PhotonView>().RequestOwnership();
            }
        }
    }
    
}