using Photon.Pun;
using UnityEngine;

public enum WeaponFireType
{
    SINGLE,
    MUTILPLE
}

public class Weapon : MonoBehaviourPunCallbacks
{
    private Animator animator;
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private GameObject bulletPrefab;
    public WeaponFireType fireType;
    private Transform firePoint;

    public float fireRate = 5f;
    public int numBullet = 30;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        firePoint = transform.Find("FirePoint");
    }

    public void Shoot(Vector2 dir)
    {
        if (numBullet < 0)
        {   
            if (photonView.IsMine)
            {
                photonView.RPC("DestroyWeapon", RpcTarget.MasterClient);
            }
            return;
        }

        if (PhotonNetwork.IsConnected)
        {
            // Gửi yêu cầu bắn lên Master Client với thông tin shooter
            int shooterID = GetComponentInParent<PhotonView>().ViewID;
            photonView.RPC("RequestShoot", RpcTarget.MasterClient, firePoint.position, firePoint.rotation, dir, shooterID);
        }
        else
        {
            // Offline mode - tạo bullet local
            SpawnBullet(firePoint.position, firePoint.rotation, dir);
        }
        
        numBullet--;
    }

    [PunRPC]
    void RequestShoot(Vector3 position, Quaternion rotation, Vector2 direction, int shooterViewID)
    {
        // Chỉ Master Client thực hiện
        if (!PhotonNetwork.IsMasterClient) return;

        GameObject bullet;
    
        if (BulletPool.Instance != null)
        {
            // Dùng pooling
            bullet = BulletPool.Instance.GetBullet();
            bullet.transform.position = position;
            bullet.transform.rotation = rotation;
        }
        else
        {
            // Fallback - instantiate mới
            bullet = PhotonNetwork.Instantiate(bulletPrefab.name, position, rotation);
        }
    
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.InitializeBullet(direction, shooterViewID);
    }

    [PunRPC]
    void DestroyWeapon()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private void SpawnBullet(Vector3 position, Quaternion rotation, Vector2 direction)
    {
        // Offline mode - tạo bullet local và initialize nó
        GameObject bullet = Instantiate(bulletPrefab, position, rotation);
        
        // Giả lập shooter ID cho offline mode (có thể dùng -1 hoặc 0)
        int offlineShooterID = -1;
        bullet.GetComponent<Bullet>().InitializeBullet(direction, offlineShooterID);
    }
}
