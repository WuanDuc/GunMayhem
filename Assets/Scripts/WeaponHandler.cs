using UnityEngine;
using Photon.Pun;

public class WeaponHandler : MonoBehaviourPunCallbacks
{
    private Transform weaponManager;
    private GameObject weapon;
    public GameObject boomPrefab;

    private float nextTimeToFire;
    [SerializeField] private int boomNum = 5;
    private float boomCountDown = 2f;
    private float boomTimer;

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
        control.Land.ThrowBoom.performed += ctx => ThrowBoom();
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
            boomTimer -= Time.deltaTime;
        }
    }

    // Method để destroy weapon khi player chết
    public void DestroyCurrentWeapon()
    {
        if (weapon != null)
        {
            // Nếu weapon có PhotonView thì destroy qua network
            PhotonView weaponPV = weapon.GetComponent<PhotonView>();
            if (weaponPV != null)
            {
                PhotonNetwork.Destroy(weapon);
            }
            else
            {
                // Nếu không có PhotonView thì destroy local
                Destroy(weapon);
            }
            weapon = null;
        }
    }

    // Method được gọi từ RandomBox
    public void EquipWeapon(GameObject newWeapon)
    {
        // Destroy weapon cũ trước khi equip weapon mới
        if (weapon != null)
        {
            PhotonView weaponPV = weapon.GetComponent<PhotonView>();
            if (weaponPV != null)
            {
                PhotonNetwork.Destroy(weapon);
            }
            else
            {
                Destroy(weapon);
            }
        }
        
        weapon = newWeapon;
        weapon.transform.parent = weaponManager;
        Vector3 pos = Vector3.zero;
        pos.z = 1;
        weapon.transform.localPosition = pos;
        weapon.transform.localScale = Vector3.one;
        weapon.GetComponent<BoxCollider2D>().enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Weapon"))
        {
            EquipWeapon(collision.gameObject);
        }
        // RandomBox logic đã được move sang RandomBox.cs
    }

    void Shoot()
    {
        if (weapon == null) return;

        Weapon wp = weapon.GetComponent<Weapon>();
        if (wp != null)
        {
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

    void ThrowBoom()
    {
        if (boomNum <= 0) return;

        if (boomTimer < 0)
        {
            if (PhotonNetwork.IsConnected)
            {
                // Master Client authority cho boom
                photonView.RPC("RequestThrowBoom", RpcTarget.MasterClient, transform.position);
            }
            else
            {
                // Offline mode
                Instantiate(boomPrefab, transform.position, Quaternion.identity);
            }
            boomNum--;
            boomTimer = boomCountDown;
        }
    }

    [PunRPC]
    void RequestThrowBoom(Vector3 position)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        GameObject boom = PhotonNetwork.Instantiate(boomPrefab.name, position, Quaternion.identity);
    }
}
