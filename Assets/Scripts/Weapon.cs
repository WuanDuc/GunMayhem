using UnityEngine;
using Photon.Pun;
public enum WeaponFireType
{
    SINGLE,
    MUTILPLE
}

public class Weapon : MonoBehaviour
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
            Destroy(gameObject);
            return;
        };
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
        // animator.SetTrigger("Shoot");
        
        numBullet--;
        Debug.Log("Shoot");
    }

}
