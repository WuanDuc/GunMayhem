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
    void EquipWeapon(GameObject newWeapon)
    {
        if (weapon != null)
        {
            Destroy(weapon);
        }
        weapon = newWeapon;
        weapon.transform.parent = weaponManager;
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localScale = Vector3.one;
        weapon.GetComponent<BoxCollider2D>().enabled = false;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Weapon"))
        {
            EquipWeapon(collision.gameObject);
        }
        if (collision.CompareTag("RandomBox"))
        {
            GameObject wp = Instantiate(collision.gameObject.GetComponent<RandomBox>().GetRamdomGun());
            Destroy(collision.gameObject);
            EquipWeapon(wp);
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
                wp.Shoot(weapon.transform.position - transform.position);
                //GameObject bullet = Instantiate(bulletPrefab, weaponManager.position, weaponManager.rotation);
                // bullet.GetComponent<Bullet>().SetShootDirection(weapon.transform.position - transform.position);
                //Debug.Log(weaponManager.position);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.J) && Time.time > nextTimeToFire)
            {
                nextTimeToFire = Time.time + 1f / wp.fireRate;
                wp.Shoot(weapon.transform.position - transform.position);
                //GameObject bullet = Instantiate(bulletPrefab, weaponManager.position, weaponManager.rotation);
                //bullet.GetComponent<Bullet>().SetShootDirection(weapon.transform.position - transform.position);
                //Debug.Log(weaponManager.position);
            }
        }
    }

}
