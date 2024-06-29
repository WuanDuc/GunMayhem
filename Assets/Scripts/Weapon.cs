using UnityEngine;

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
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation );
        bullet.GetComponent<Bullet>().SetShootDirection(dir);
        // animator.SetTrigger("Shoot");
        numBullet--;
        Debug.Log("Shoot");
    }

}
