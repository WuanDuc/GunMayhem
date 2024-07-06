using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum BulletType
{
    NORMAL,
    SHOTGUN
}

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
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
        if (!PhotonNetwork.IsConnected || photonView.IsMine)
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
            collision.GetComponent<KnockBackHandler>().KnockBack(direction, force);
            if (PhotonNetwork.IsConnected)
            {
                Debug.Log("Calling DestroyBullet RPC.");
                photonView.RPC("DestroyBullet", RpcTarget.AllBuffered);
            }
            else
            {
                Debug.Log("Destroying bullet locally.");
                Destroy(gameObject);
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
    [PunRPC]
    public void DestroyBullet()
    {
        Debug.Log("DestroyBullet called. PhotonView is mine: " + photonView.IsMine);
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
