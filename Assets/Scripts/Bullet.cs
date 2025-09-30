using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Collections;

public enum BulletType
{
    NORMAL,
    SHOTGUN
}

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    private Vector2 direction;
    public float force = 10f;
    public BulletType type;
    PhotonView photonView;
    
    // FIXED: Track shooter to prevent self-damage
    private int shooterViewID = -1;
    
    private void Start()
    {
        photonView = GetComponent<PhotonView>();    
    }
    
    public void SetShooterViewID(int viewID)
    {
        shooterViewID = viewID;
        Debug.Log("Bullet shooter set to ViewID: " + shooterViewID);
    }
    
    void Update()
    {
        // Only Master Client handles bullet movement
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient) return;
        
        switch (type)
        {
            case BulletType.NORMAL:
                transform.Translate(speed * Time.deltaTime * direction);
                break;
            case BulletType.SHOTGUN:
                // Add Shotgun specific behavior if any
                break;
        }
    }

    public void SetShootDirection(Vector2 direction)
    {
        this.direction = direction.normalized;
        this.direction.y = 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PhotonView targetPhotonView = collision.GetComponent<PhotonView>();
            
            // FIXED: Don't ignore own bullets - let knockback handler decide
            if (photonView.Owner != null && targetPhotonView.Owner != null && photonView.Owner == targetPhotonView.Owner)
            {
                Debug.Log("Ignoring collision with bullet owner");
                return;
            }
            
            // FIXED: Apply knockback to ALL players, not just bullet owner
            targetPhotonView.RPC("ApplyKnockBack", RpcTarget.All, direction, force);
            
            if (photonView.IsMine)
            {
                DestroyBullet();
            }
        }
        
        // Handle collision with environment/walls
        else if (collision.CompareTag("Ground") || collision.CompareTag("Wall"))
        {
            Debug.Log("Bullet hit environment: " + collision.tag);
            DestroyBullet();
        }
    }

    public void ShotgunKnockBack()
    {
        // Keep as is for now - not used in current implementation
    }
    
    private void DestroyBullet()
    {
        Debug.Log("Destroying bullet");
        if (PhotonNetwork.IsConnected)
        {
            if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                // Request destruction if not owned by Master Client
                photonView.RPC("RequestDestroyBullet", photonView.Owner, photonView.ViewID);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [PunRPC]
    public void RequestDestroyBullet(int viewID)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        PhotonView targetView = PhotonView.Find(viewID);
        if (targetView != null && targetView.IsMine)
        {
            PhotonNetwork.Destroy(targetView.gameObject);
        }
        else
        {
            Debug.LogWarning("Requested PhotonView not found or not owned by this client.");
        }
    }
}
