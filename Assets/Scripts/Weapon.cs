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

    public float fireRate = 5f;
    public int numBullet = 30;
    private void Awake()
    {
        animator = GetComponent<Animator>();

    }

    public void Shoot(Vector2 dir)
    {
        if (numBullet < 0)
        {   
            Destroy(gameObject);
            return;
        };
        GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
        bullet.GetComponent<Bullet>().SetShootDirection(dir);
        Debug.Log(bullet.transform.position);
        // animator.SetTrigger("Shoot");
        
        numBullet--;
    }

}
