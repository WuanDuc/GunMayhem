using UnityEngine;
using Photon.Pun;

public class WeaponHandler : MonoBehaviourPun
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
    private PlayerMovement playerMovement;
    
    private void Awake()
    {
        weaponManager = transform.Find("WeaponManager");
        view = GetComponent<PhotonView>();
        control = new InputSystem();
        playerMovement = GetComponent<PlayerMovement>();
        
        if (weaponManager.childCount > 0)
        {
            weapon = weaponManager.GetChild(0).gameObject;
        }
    }

    private void Start()
    {
        if (!view.IsMine)
        {
            control.Disable();
            return;
        }
    }

    void Update()
    {
        if (view.IsMine)
        {
            HandleWeaponInput();
        }

        // FIXED: Update weapon direction for ALL players every frame
        UpdateWeaponDirection();

        if (PhotonNetwork.IsMasterClient && boomTimer > 0)
        {
            boomTimer -= Time.deltaTime;
        }
    }

    // FIXED: Improved weapon direction logic
    private void UpdateWeaponDirection()
    {
        if (weapon != null && playerMovement != null)
        {
            bool isFacingRight = playerMovement.IsFacingRight();
            
            Vector3 weaponPos = weapon.transform.localPosition;
            
            if (isFacingRight)
            {
                weaponPos.x = Mathf.Abs(weaponPos.x);
                weapon.transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                weaponPos.x = -Mathf.Abs(weaponPos.x);
                weapon.transform.localScale = new Vector3(-1, 1, 1);
            }
            
            weapon.transform.localPosition = weaponPos;
        }
    }

    private void HandleWeaponInput()
    {
        bool firePressed = control.Land.Shoot.triggered;
        bool boomPressed = control.Land.ThrowBoom.triggered;

        if (firePressed && weapon != null)
        {
            // FIXED: Master Client processes directly, others send RPC
            if (view.IsMine && PhotonNetwork.IsMasterClient)
            {
                ProcessFireInput(Time.time); // Direct processing
            }
            else if (view.IsMine && !PhotonNetwork.IsMasterClient)
            {
                view.RPC("ReceiveFireInput", RpcTarget.MasterClient, Time.time);
            }
        }

        if (boomPressed && boomNum > 0)
        {
            if (view.IsMine && PhotonNetwork.IsMasterClient)
            {
                ProcessBoomInput(transform.position); // Direct processing
            }
            else if (view.IsMine && !PhotonNetwork.IsMasterClient)
            {
                view.RPC("ReceiveBoomInput", RpcTarget.MasterClient, transform.position);
            }
        }
    }

    [PunRPC]
    void ReceiveFireInput(float inputTime)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        ProcessFireInput(inputTime);
    }

    private void ProcessFireInput(float inputTime)
    {
        if (weapon == null) return;
        
        if (inputTime > nextTimeToFire)
        {
            // FIXED: Get CURRENT shooting direction at the moment of firing
            Vector2 shootDirection = playerMovement.IsFacingRight() ? Vector2.right : Vector2.left;
            
            Debug.Log($"Player {view.Owner?.NickName} shooting direction: {shootDirection}, facing right: {playerMovement.IsFacingRight()}");
            
            weapon.GetComponent<Weapon>().Shoot(shootDirection);
            nextTimeToFire = inputTime + 1f / weapon.GetComponent<Weapon>().fireRate;
            
            if (view.IsMine)
            {
                SoundManager.PlaySound(SoundManager.Sound.Fire);
            }
            
            Debug.Log($"Master Client processed fire for player: {view.Owner?.NickName}");
        }
    }

    [PunRPC]
    void ReceiveBoomInput(Vector3 spawnPosition)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        ProcessBoomInput(spawnPosition);
    }

    private void ProcessBoomInput(Vector3 spawnPosition)
    {
        if (boomNum <= 0 || boomTimer > 0) return;
        
        boomTimer = boomCountDown;
        boomNum--;
        
        GameObject boom = PhotonNetwork.Instantiate(boomPrefab.name, spawnPosition, Quaternion.identity);
        Vector2 throwDirection = playerMovement.IsFacingRight() ? Vector2.right : Vector2.left;
        boom.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 4f + throwDirection * 3f, ForceMode2D.Impulse);
        
        view.RPC("SyncBoomCount", view.Owner, boomNum, boomTimer);
        
        Debug.Log($"Master Client spawned boom for player: {view.Owner?.NickName}");
    }

    [PunRPC]
    void SyncBoomCount(int newBoomNum, float newBoomTimer)
    {
        if (!view.IsMine) return;
        
        boomNum = newBoomNum;
        boomTimer = newBoomTimer;
        
        Debug.Log($"Boom count synced: {boomNum} remaining");
    }

    public void DestroyCurrentWeapon()
    {
        if (weapon != null)
        {
            Debug.Log($"Destroying weapon for player: {view.Owner?.NickName}");
            
            if (PhotonNetwork.IsConnected)
            {
                PhotonView weaponPhotonView = weapon.GetComponent<PhotonView>();
                if (weaponPhotonView != null && weaponPhotonView.IsMine)
                {
                    PhotonNetwork.Destroy(weapon);
                }
                else if (weaponPhotonView != null)
                {
                    weaponPhotonView.RPC("RequestDestroyWeapon", RpcTarget.MasterClient, weaponPhotonView.ViewID);
                }
                else
                {
                    Destroy(weapon);
                }
            }
            else
            {
                Destroy(weapon);
            }
            
            weapon = null;
        }
    }

    void EquipWeapon(GameObject newWeapon)
    {
        if (weapon != null)
        {
            DestroyCurrentWeapon();
        }
        
        if (PhotonNetwork.IsConnected)
        {
            weapon = PhotonNetwork.Instantiate(newWeapon.name, weaponManager.position, weaponManager.rotation);
            weapon.transform.SetParent(weaponManager);
            
            Vector3 pos = Vector3.zero;
            pos.z = 1;
            weapon.transform.localPosition = pos;
            weapon.transform.localScale = Vector3.one;
            
            BoxCollider2D weaponCollider = weapon.GetComponent<BoxCollider2D>();
            if (weaponCollider != null)
            {
                weaponCollider.enabled = false;
            }
            
            // FIXED: Force immediate weapon direction update
            UpdateWeaponDirection();
            
            view.RPC("SyncWeaponVisual", RpcTarget.Others, newWeapon.name, weapon.GetComponent<PhotonView>().ViewID);
        }
        else
        {
            weapon = Instantiate(newWeapon, weaponManager);
            Vector3 pos = Vector3.zero;
            pos.z = 1;
            weapon.transform.localPosition = pos;
            weapon.transform.localScale = Vector3.one;
            
            BoxCollider2D weaponCollider = weapon.GetComponent<BoxCollider2D>();
            if (weaponCollider != null)
            {
                weaponCollider.enabled = false;
            }
            
            UpdateWeaponDirection();
        }
        
        Debug.Log($"Equipped new weapon: {newWeapon.name} for player: {view.Owner?.NickName}");
    }

    [PunRPC]
    void SyncWeaponVisual(string weaponName, int weaponViewID)
    {
        if (view.IsMine) return;
        
        PhotonView weaponPhotonView = PhotonView.Find(weaponViewID);
        if (weaponPhotonView != null)
        {
            if (weapon != null)
            {
                Destroy(weapon);
            }
            
            weapon = weaponPhotonView.gameObject;
            weapon.transform.SetParent(weaponManager);
            
            Vector3 pos = Vector3.zero;
            pos.z = 1;
            weapon.transform.localPosition = pos;
            weapon.transform.localScale = Vector3.one;
            
            BoxCollider2D weaponCollider = weapon.GetComponent<BoxCollider2D>();
            if (weaponCollider != null)
            {
                weaponCollider.enabled = false;
            }
            
            // FIXED: Force immediate weapon direction update for synced weapons
            UpdateWeaponDirection();
            
            Debug.Log($"Weapon visual synced: {weaponName} for remote player {view.Owner?.NickName}");
        }
    }

    [PunRPC]
    void RequestDestroyWeapon(int weaponViewID)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        PhotonView targetWeapon = PhotonView.Find(weaponViewID);
        if (targetWeapon != null)
        {
            Debug.Log($"Master Client destroying weapon ViewID: {weaponViewID}");
            PhotonNetwork.Destroy(targetWeapon.gameObject);
        }
    }

    public void SetWeapon(GameObject newWeapon)
    {
        EquipWeapon(newWeapon);
    }

    public bool HasWeapon()
    {
        return weapon != null;
    }

    public GameObject GetCurrentWeapon()
    {
        return weapon;
    }

    [PunRPC]
    public void EquipNetworkWeapon(int weaponViewID)
    {
        PhotonView weaponPhotonView = PhotonView.Find(weaponViewID);
        if (weaponPhotonView != null)
        {
            if (weapon != null)
            {
                DestroyCurrentWeapon();
            }
            
            weapon = weaponPhotonView.gameObject;
            weapon.transform.SetParent(weaponManager);
            
            Vector3 pos = Vector3.zero;
            pos.z = 1;
            weapon.transform.localPosition = pos;
            weapon.transform.localScale = Vector3.one;
            
            BoxCollider2D weaponCollider = weapon.GetComponent<BoxCollider2D>();
            if (weaponCollider != null)
            {
                weaponCollider.enabled = false;
            }
            
            // FIXED: Force immediate weapon direction update
            UpdateWeaponDirection();
            
            view.RPC("SyncWeaponVisual", RpcTarget.Others, weapon.name, weaponViewID);
            
            Debug.Log($"Network weapon equipped: {weapon.name} for player: {view.Owner?.NickName}");
        }
        else
        {
            Debug.LogError($"Could not find weapon with ViewID: {weaponViewID}");
        }
    }

    private void OnEnable()
    {
        if (view.IsMine)
        {
            control.Enable();
        }
    }

    private void OnDisable()
    {
        control.Disable();
    }
}
