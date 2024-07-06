using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    private Vector2 direction;
    public float force = 10f;
    // Update is called once per frame
    void Update()
    {
        transform.Translate(speed * Time.deltaTime * direction );
    }
    public void SetShootDirection(Vector2 direction)
    {
        this.direction = direction.normalized;
        this.direction.y = 0;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
<<<<<<< Updated upstream
        if(collision.CompareTag("Player"))
=======
        if (collision.CompareTag("Player"))
        {
            Debug.Log("bullet hit player");
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
>>>>>>> Stashed changes
        {
            collision.GetComponent<KnockBackHandler>().KnockBack(direction,force);
            Destroy(gameObject);
        }
    }
}
