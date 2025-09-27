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
    
    // NEW: Track shooter to prevent self-damage
    private int shooterViewID = -1;
    
    private void Start()
    {
        photonView = GetComponent<PhotonView>();    
    }
    
    // NEW: Method to set shooter ID (called from Weapon.cs)
    public void SetShooterViewID(int viewID)
    {
        shooterViewID = viewID;
        Debug.Log("Bullet shooter set to ViewID: " + shooterViewID);
    }
    
    // Update is called once per frame
    void Update()
    {
        // Only Master Client handles bullet movement
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient) return;
        
        switch (type)
        {
            case BulletType.NORMAL:
                //CheckIfOutOfBounds();
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
        // Only Master Client handles collision detection
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient) return;
        
        Debug.Log("Bullet collided with: " + collision.tag);
        if (collision.CompareTag("Player"))
        {
            PhotonView targetPhotonView = collision.GetComponent<PhotonView>();
            
            // NEW: Prevent self-damage - check if hit player is the shooter
            if (targetPhotonView != null && targetPhotonView.ViewID == shooterViewID)
            {
                Debug.Log("Ignoring collision with shooter (ViewID: " + shooterViewID + ")");
                return;
            }

            if (photonView != null && photonView.Owner != null)
            {
                Debug.Log("Bullet hit player. Owner: " + photonView.Owner.NickName);
            }
            
            // OLD CODE - commented out (local knockback)
            // collision.GetComponent<KnockBackHandler>().KnockBack(direction, force);
            
            // NEW: Master Client sends knockback to target player
            if (targetPhotonView != null)
            {
                targetPhotonView.RPC("ApplyKnockBack", targetPhotonView.Owner, direction, force);
                Debug.Log("Knockback RPC sent to player ViewID: " + targetPhotonView.ViewID);
            }
            
            // Master Client destroys the bullet
            if (photonView.IsMine)
            {
                DestroyBullet();
            }
            else
            {
                // Request destruction if not owned by Master Client
                photonView.RPC("RequestDestroyBullet", photonView.Owner, photonView.ViewID);
            }
        }
        
        // NEW: Handle collision with environment/walls
        else if (collision.CompareTag("Ground") || collision.CompareTag("Wall"))
        {
            Debug.Log("Bullet hit environment: " + collision.tag);
            if (photonView.IsMine)
            {
                DestroyBullet();
            }
        }
    }

    public void ShotgunKnockBack()
    {
        // OLD CODE - keep as is for now
        /*
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 1f);
        foreach (Collider2D hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                hit.gameObject.GetComponent<KnockBackHandler>().KnockBack(direction, force);
            }
        }
        */
    }
    
    private void DestroyBullet()
    {
        Debug.Log("Destroying bullet");
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [PunRPC]
    public void RequestDestroyBullet(int viewID)
    {
        PhotonView targetView = PhotonView.Find(viewID);
        if (targetView != null && targetView.IsMine)
        {
            targetView.GetComponent<Bullet>().DestroyBullet();
        }
        else
        {
            Debug.LogWarning("Requested PhotonView not found or not owned by this client.");
        }
    }
}
