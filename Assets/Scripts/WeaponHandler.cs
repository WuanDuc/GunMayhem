using UnityEngine;
using Photon.Pun;
public class WeaponHandler : MonoBehaviour
{
    private Transform weaponManager;
    private GameObject weapon;
    public GameObject bulletPrefab;

    private float nextTimeToFire;

    private PhotonView view;
    private InputSystem control;
    private void Awake()
    {
        weaponManager = transform.Find("WeaponManager");
        if (weaponManager.childCount > 0)
        {
            weapon = weaponManager.GetChild(0).gameObject;
        }
        control = new InputSystem();
        control.Enable();

        control.Land.Shoot.performed += ctx => Shoot();
    }
    private void Start()
    {
        view = GetComponent<PhotonView>();
        if (!view.IsMine)
        {
            Destroy(this);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (view.IsMine)
        {
            //Shoot();
        }
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
    //void Shoot()
    //{
    //    if (weapon == null)
    //        return;
    //    Weapon wp = weapon.GetComponent<Weapon>();
    //    if (wp.fireType == WeaponFireType.MUTILPLE)
    //    {
    //        if (Input.GetKey(KeyCode.J) && Time.time > nextTimeToFire)
    //        {
    //            nextTimeToFire = Time.time + 1f / wp.fireRate;
    //            wp.Shoot(weapon.transform.position - transform.position);

    //        }
    //    }
    //    else
    //    {
    //        if (Input.GetKeyDown(KeyCode.J) && Time.time > nextTimeToFire)
    //        {
    //            nextTimeToFire = Time.time + 1f / wp.fireRate;
    //            wp.Shoot(weapon.transform.position - transform.position);

    //        }
    //    }
    //}
    void Shoot()
    {
        if (view.IsMine && weapon != null)
        {
            Weapon wp = weapon.GetComponent<Weapon>();
            if (wp.fireType == WeaponFireType.MUTILPLE)
            {
                if (Time.time > nextTimeToFire)
                {
                    nextTimeToFire = Time.time + 1f / wp.fireRate;
                    wp.Shoot(weapon.transform.position - transform.position);
                }
            }
            else
            {
                if (Time.time > nextTimeToFire)
                {
                    nextTimeToFire = Time.time + 1f / wp.fireRate;
                    wp.Shoot(weapon.transform.position - transform.position);
                }
            }
        }
    }
}
