using UnityEngine;
using Photon.Pun;
public class WeaponHandler : MonoBehaviour
{
    private Transform weaponManager;
    private GameObject weapon;
    public GameObject boomPrefab;

    private float nextTimeToFire;
    [SerializeField] private int boomNum=5;
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
    // Update is called once per frame
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
        Vector3 vector3 = Vector3.zero;
        vector3.z = 1;
        weapon.transform.localPosition = vector3;
        
        weapon.transform.localScale = Vector3.one;
        weapon.GetComponent<BoxCollider2D>().enabled = false;
        view.RPC("DestroyWeaponAcrossNetwork", RpcTarget.All, weapon.GetComponent<PhotonView>().ViewID);
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
            //Destroy(collision.gameObject);
            view.RPC("DestroyRandomBoxAcrossNetwork", RpcTarget.AllBuffered, collision.gameObject.GetComponent<PhotonView>().ViewID);
            Destroy(collision.gameObject);
            EquipWeapon(wp);
        }
    }
    [PunRPC]
    void DestroyRandomBoxAcrossNetwork(int viewID)
    {
        PhotonView pv = PhotonView.Find(viewID);
        if (pv != null)
        {
            GameObject randomBoxToDestroy = pv.gameObject;
            if (randomBoxToDestroy != null)
            {
                Destroy(randomBoxToDestroy);
            }
        }
        else
        {
            Debug.LogWarning($"PhotonView with ID {viewID} not found.");
        }
    }
    void Shoot()
    {
        if (weapon == null)
            return;

        Weapon wp = weapon.GetComponent<Weapon>();
        if (wp == null)
        {
            Debug.LogError("Weapon component is not found on the weapon object!");
            return;
        }

        if (wp.fireType == WeaponFireType.MUTILPLE)
        {
            if (Input.GetKey(KeyCode.J) && Time.time > nextTimeToFire)
            {
                nextTimeToFire = Time.time + 1f / wp.fireRate;
                Vector3 position = weapon.transform.position;
                position.z = 1f;
                Vector2 direction = weapon.transform.position - transform.position;
                view.RPC("ShootBullet", RpcTarget.All, position, direction);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.J) && Time.time > nextTimeToFire)
            {
                nextTimeToFire = Time.time + 1f / wp.fireRate;
                Vector3 position = weapon.transform.position;
                position.z = 1f;
                Vector2 direction = weapon.transform.position - transform.position;
                view.RPC("ShootBullet", RpcTarget.All, position, direction);
            }
        }
    }
    void ThrowBoom()
    {
        if (boomNum <= 0)
            return;
        
        boomTimer -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.K)&&boomTimer<0)
        {
            GameObject boom = Instantiate(boomPrefab, transform.position, transform.rotation);
            Vector3 throwPosition = transform.position;
            Vector2 throwDirection = gameObject.GetComponent<PlayerMovement>().IsFacingRight() ? Vector2.right : Vector2.left;
            boom.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 4f+ throwDirection * 3f, ForceMode2D.Impulse);
            view.RPC("ThrowBoomRPC", RpcTarget.All, throwPosition, throwDirection);
            boomTimer = boomCountDown;
            boomNum--;
        }
    }
    [PunRPC]
    void ThrowBoomRPC(Vector3 position, Vector2 direction)
    {
        GameObject boom = PhotonNetwork.Instantiate("Boom", position, Quaternion.identity);
        boom.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 4f + direction * 3f, ForceMode2D.Impulse);
    }
    [PunRPC]
    void DestroyWeaponAcrossNetwork(int viewID)
    {
        PhotonView pv = PhotonView.Find(viewID);
        if (pv != null)
        {
            GameObject weaponToDestroy = pv.gameObject;
            if (weaponToDestroy != null)
            {
                Destroy(weaponToDestroy);
            }
        }
        else
        {
            Debug.LogWarning($"PhotonView with ID {viewID} not found.");
        }
    }

    [PunRPC]
    void ShootBullet(Vector3 position, Vector2 direction)
    {
        GameObject bullet = PhotonNetwork.Instantiate("Bullet", position, Quaternion.identity);
        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        bulletComponent.SetShootDirection(direction);
    }

}
