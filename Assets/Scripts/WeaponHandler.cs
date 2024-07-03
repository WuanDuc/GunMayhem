using UnityEngine;
using Photon.Pun;

public class WeaponHandler : MonoBehaviour
{
    private Transform weaponManager;
    private GameObject weapon;
    public GameObject boomPrefab;

    private float nextTimeToFire;
    [SerializeField] private int boomNum = 5;
    private float boomCountDown = 2f;
    private float boomTimer;

    private PhotonView view;

    private void Awake()
    {
        weaponManager = transform.Find("WeaponManager");
        if (weaponManager.childCount > 0)
        {
            weapon = weaponManager.GetChild(0).gameObject;
        }
    }

    private void Start()
    {
        view = GetComponent<PhotonView>();
        if (!view.IsMine)
        {
            Destroy(this);
        }
    }

    void Update()
    {
        if (view.IsMine)
        {
            Shoot();
            ThrowBoom();
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
            string randomGunName = collision.gameObject.GetComponent<RandomBox>().GetRamdomGun().name;
            //GameObject wp = Instantiate(collision.gameObject.GetComponent<RandomBox>().GetRamdomGun());
            // Instantiate the weapon across the network
            GameObject wp = PhotonNetwork.Instantiate(randomGunName, transform.position, Quaternion.identity);

            // Set the position and scale of the instantiated weapon
            wp.transform.position = weaponManager.position;
            wp.transform.localScale = Vector3.one;
            PhotonNetwork.Destroy(collision.gameObject);
            EquipWeapon(wp);
        }
    }

    void Shoot()
    {
        if (weapon == null)
            return;

        Weapon wp = weapon.GetComponent<Weapon>();
        if (wp != null)
        {
            if (wp.fireType == WeaponFireType.MUTILPLE)
            {
                if (Input.GetKey(KeyCode.J) && Time.time > nextTimeToFire)
                {
                    nextTimeToFire = Time.time + 1f / wp.fireRate;
                    wp.Shoot(weapon.transform.position - transform.position);

                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.J) && Time.time > nextTimeToFire)
                {
                    nextTimeToFire = Time.time + 1f / wp.fireRate;
                    wp.Shoot(weapon.transform.position - transform.position);

                }
            }
        }

    }

    void ThrowBoom()
    {
        if (boomNum <= 0)
            return;

        boomTimer -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.K) && boomTimer < 0)
        {
            GameObject boom = PhotonNetwork.Instantiate(boomPrefab.name, transform.position, transform.rotation);

            Vector2 throwDirection = gameObject.GetComponent<PlayerMovement>().IsFacingRight() ? Vector2.right : Vector2.left;
            boom.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 4f + throwDirection * 3f, ForceMode2D.Impulse);
            boomTimer = boomCountDown;
            boomNum--;
        }
    }
}
