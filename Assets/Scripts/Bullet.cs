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
    private void Start()
    {
        photonView = GetComponent<PhotonView>();    
    }
    // Update is called once per frame
    void Update()
    {
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
        Debug.Log("Bullet collided with: " + collision.tag);
        if (collision.CompareTag("Player"))
        {

            if (photonView != null && photonView.Owner != null)
            {
                Debug.Log("Bullet hit player. Owner: " + photonView.Owner.NickName);
            }
            collision.GetComponent<KnockBackHandler>().KnockBack(direction, force);
            PhotonView targetPhotonView = collision.GetComponent<PhotonView>();
            if (photonView.Owner != null && targetPhotonView.Owner != null && photonView.Owner == targetPhotonView.Owner)
            {
                Debug.Log("Ignoring collision with the player who owns the bullet.");
                return;
            }
            if (targetPhotonView != null)
            {
                targetPhotonView.RPC("ApplyKnockBack", targetPhotonView.Owner, direction, force);
            }
            if (photonView.IsMine)
            {
                DestroyBullet();
            }
            else
            {
                photonView.RPC("RequestDestroyBullet", photonView.Owner, photonView.ViewID);
            }
        }
    }

    public void ShotgunKnockBack()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 0.2f);
        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Player"))
            {
                //collider.GetComponent<PhotonView>().RPC("ApplyKnockBack", RpcTarget.AllBuffered, direction, force);
                collider.gameObject.GetComponent<KnockBackHandler>().KnockBack(direction, force);
            }
        }
    }
    private void DestroyBullet()
    {
        Debug.Log("Destroying Bullet owned by: " + photonView.OwnerActorNr);
        PhotonNetwork.Destroy(gameObject);
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
