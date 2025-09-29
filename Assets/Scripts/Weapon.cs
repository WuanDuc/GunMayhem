using UnityEngine;
using Photon.Pun;

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
    
    // NEW: Track the shooter's PhotonView ID
    private int shooterViewID;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        firePoint = transform.Find("FirePoint");
    }

    private void Start()
    {
        // Get the shooter's PhotonView ID from the parent player
        PhotonView playerPhotonView = GetComponentInParent<PhotonView>();
        if (playerPhotonView != null)
        {
            shooterViewID = playerPhotonView.ViewID;
        }
    }

    public void Shoot(Vector2 dir)
    {
        if (numBullet <= 0)
        {   
            Destroy(gameObject);
            return;
        }

        // NEW: Send shoot request to Master Client instead of direct instantiation
        Vector3 bulletPosition = firePoint.position;
        bulletPosition.z = 1;
        
        if (PhotonNetwork.IsConnected)
        {
            // Send RPC to Master Client to handle bullet spawning
            photonView.RPC("RequestBulletSpawn", RpcTarget.MasterClient, 
                bulletPosition, firePoint.rotation, dir, shooterViewID);
        }
        else
        {
            // Offline mode - direct instantiation
            GameObject bullet = Instantiate(bulletPrefab, bulletPosition, firePoint.rotation);
            bullet.GetComponent<Bullet>().SetShootDirection(dir);
        }

        // OLD CODE - commented out
        /*
        Vector3 bulletPosition = firePoint.position;
        bulletPosition.z = 1;
        GameObject bullet;
        if (PhotonNetwork.IsConnected) {
             bullet = PhotonNetwork.Instantiate(bulletPrefab.name, bulletPosition, firePoint.rotation);
            Debug.Log("Photon shot bullet! Owner: " + bullet.GetComponent<PhotonView>().Owner.NickName);
        }
        else
        {
            bullet = Instantiate(bulletPrefab, bulletPosition, firePoint.rotation);
        }
        bullet.GetComponent<Bullet>().SetShootDirection(dir);
        Debug.Log(bullet.transform.position);
        */
        
        // animator.SetTrigger("Shoot");
        numBullet--;
        Debug.Log("Shoot request sent");
    }

    [PunRPC]
    void RequestBulletSpawn(Vector3 position, Quaternion rotation, Vector2 direction, int shooterID)
    {
        // Only Master Client processes this request
        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log("Master Client spawning bullet for shooter ID: " + shooterID);
        
        // Spawn bullet on Master Client
        GameObject bullet = PhotonNetwork.Instantiate(bulletPrefab.name, position, rotation);
        
        // Set bullet properties
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetShootDirection(direction);
            bulletScript.SetShooterViewID(shooterID); // NEW: Set shooter ID to prevent self-damage
        }
        
        Debug.Log("Bullet spawned by Master Client");
    }
}
