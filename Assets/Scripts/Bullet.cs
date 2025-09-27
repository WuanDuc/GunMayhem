using System.Collections;
using UnityEngine;
using Photon.Pun;

public enum BulletType
{
    NORMAL,
    SHOTGUN
}

public class Bullet : MonoBehaviourPunCallbacks
{
    public float speed = 10f;
    private Vector2 direction;
    public float force = 10f;
    public BulletType type;
    
    private int shooterViewID;
    private float lifeTime = 5f; // Auto destroy after 5 seconds
    private float currentLifeTime;
    
    [PunRPC]
    public void InitializeBullet(Vector2 dir, int shooterID)
    {
        direction = dir.normalized;
        direction.y = 0;
        shooterViewID = shooterID;
        currentLifeTime = lifeTime;
    }

    public void ResetBullet()
    {
        direction = Vector2.zero;
        shooterViewID = 0;
        currentLifeTime = lifeTime;
    }

    void Update()
    {
        // CHỈ Master Client di chuyển bullet
        if (PhotonNetwork.IsMasterClient)
        {
            transform.Translate(speed * Time.deltaTime * direction);
            
            // Auto return to pool after lifetime
            currentLifeTime -= Time.deltaTime;
            if (currentLifeTime <= 0)
            {
                ReturnToPool();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // CHỈ Master Client xử lý va chạm
        if (!PhotonNetwork.IsMasterClient) return;
        
        if (collision.CompareTag("Player"))
        {
            PhotonView targetPV = collision.GetComponent<PhotonView>();
            if (targetPV != null && targetPV.ViewID != shooterViewID)
            {
                photonView.RPC("ProcessHit", RpcTarget.All, targetPV.ViewID, direction, force, shooterViewID);
            }
            
            ReturnToPool();
        }
        else if (collision.CompareTag("Ground") || collision.CompareTag("Wall"))
        {
            ReturnToPool();
        }
    }

    void ReturnToPool()
    {
        // Thông báo cho tất cả client return bullet
        photonView.RPC("OnBulletDestroyed", RpcTarget.All);
    }

    [PunRPC]
    void OnBulletDestroyed()
    {
        if (BulletPool.Instance != null)
        {
            BulletPool.Instance.ReturnBullet(gameObject);
        }
        else
        {
            // Fallback nếu không có pool
            Destroy(gameObject);
        }
    }

    [PunRPC]
    void ProcessHit(int targetViewID, Vector2 hitDirection, float hitForce, int shooterID)
    {
        PhotonView target = PhotonView.Find(targetViewID);
        PhotonView shooter = PhotonView.Find(shooterID);
        
        if (target != null)
        {
            target.GetComponent<KnockBackHandler>()?.KnockBack(hitDirection, hitForce);
            
            if (target.IsMine)
            {
                UpdatePlayerStats(shooter?.Owner, target.Owner);
            }
        }
    }
    
    private void UpdatePlayerStats(Photon.Realtime.Player shooter, Photon.Realtime.Player target)
    {
        if (shooter != null && target != null)
        {
            var shooterProps = shooter.CustomProperties;
            shooterProps["kills"] = (int)(shooterProps["kills"] ?? 0) + 1;
            shooter.SetCustomProperties(shooterProps);
            
            var targetProps = target.CustomProperties;
            targetProps["deaths"] = (int)(targetProps["deaths"] ?? 0) + 1;
            target.SetCustomProperties(targetProps);
        }
    }
}
