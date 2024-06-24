using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    private Transform weaponManager;
    private GameObject weapon;
    public GameObject bulletPrefab;

    private float nextTimeToFire;
    private void Awake()
    {
        weaponManager = transform.Find("WeaponManager");
        if (weaponManager.childCount > 0)
        {
            weapon = weaponManager.GetChild(0).gameObject;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Shoot();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Weapon"))
        {
            if (weapon != null)
            {
                Destroy(weapon);
            }
            weapon = collision.gameObject;
            weapon.transform.parent = weaponManager;
            weapon.transform.localPosition = Vector3.zero;
            weapon.GetComponent<BoxCollider2D>().enabled = false;

        }
    }
    void Shoot()
    {
        if (weapon == null)
            return;
        Weapon wp = weapon.GetComponent<Weapon>();
        if (wp.fireType == WeaponFireType.MUTILPLE)
        {
            if (Input.GetKey(KeyCode.J) && Time.time > nextTimeToFire)
            {
                nextTimeToFire = Time.time + 1f / wp.fireRate;
                wp.ShootAnimation();
                GameObject bullet = Instantiate(bulletPrefab, weaponManager.position, weaponManager.rotation);
                bullet.GetComponent<Bullet>().SetShootDirection(weapon.transform.position - transform.position);

            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.J) && Time.time > nextTimeToFire)
            {
                nextTimeToFire = Time.time + 1f / wp.fireRate;
                //wp.ShootAnimation();
                GameObject bullet = Instantiate(bulletPrefab, weaponManager.position, weaponManager.rotation);
                bullet.GetComponent<Bullet>().SetShootDirection(weapon.transform.position - transform.position);

            }
        }
    }

}
