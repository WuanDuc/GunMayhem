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
    
    // NEW: Reference to player movement for facing direction
    private PlayerMovement playerMovement;
    
    private void Awake()
    {
        weaponManager = transform.Find("WeaponManager");
        view = GetComponent<PhotonView>();
        control = new InputSystem();
        playerMovement = GetComponent<PlayerMovement>(); // Get player movement component
        
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

        // FIXED: Update weapon direction for all players
        UpdateWeaponDirection();

        if (PhotonNetwork.IsMasterClient && boomTimer > 0)
        {
            boomTimer -= Time.deltaTime;
        }
    }

    // NEW: Update weapon direction based on player facing
    private void UpdateWeaponDirection()
    {
        if (weapon != null && playerMovement != null)
        {
            // Get player facing direction
            bool isFacingRight = playerMovement.IsFacingRight();
            
            // Flip weapon based on player direction
            Vector3 weaponScale = weapon.transform.localScale;
            
            if (isFacingRight && weaponScale.x < 0)
            {
                weaponScale.x = Mathf.Abs(weaponScale.x);
                weapon.transform.localScale = weaponScale;
            }
            else if (!isFacingRight && weaponScale.x > 0)
            {
                weaponScale.x = -Mathf.Abs(weaponScale.x);
                weapon.transform.localScale = weaponScale;
            }
        }
    }

    private void HandleWeaponInput()
    {
        bool firePressed = control.Land.Shoot.triggered;
        bool boomPressed = control.Land.ThrowBoom.triggered;

        if (firePressed && weapon != null)
        {
            if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
            {
                view.RPC("ReceiveFireInput", RpcTarget.MasterClient, Time.time);
            }
            else if (PhotonNetwork.IsMasterClient)
            {
                ProcessFireInput(Time.time);
            }
        }

        if (boomPressed && boomNum > 0)
        {
            if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
            {
                view.RPC("ReceiveBoomInput", RpcTarget.MasterClient, transform.position);
            }
            else if (PhotonNetwork.IsMasterClient)
            {
                ProcessBoomInput(transform.position);
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
            // FIXED: Get proper shooting direction based on player facing
            Vector2 shootDirection = playerMovement.IsFacingRight() ? Vector2.right : Vector2.left;
            
            weapon.GetComponent<Weapon>().Shoot(shootDirection);
            nextTimeToFire = inputTime + 1f / weapon.GetComponent<Weapon>().fireRate;
            
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

    // FIXED: Proper weapon equipping with network synchronization
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
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
            
            // FIXED: Set initial weapon direction
            UpdateWeaponDirection();
            
            // Sync weapon visual to all clients
            view.RPC("SyncWeaponVisual", RpcTarget.Others, newWeapon.name, weapon.GetComponent<PhotonView>().ViewID);
        }
        else
        {
            weapon = Instantiate(newWeapon, weaponManager);
            UpdateWeaponDirection();
        }
        
        Debug.Log($"Equipped new weapon: {newWeapon.name} for player: {view.Owner?.NickName}");
    }

    // FIXED: New RPC to sync weapon visuals across all clients
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
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
            
            // FIXED: Set initial weapon direction for remote players
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

    // FIXED: This is called by RandomBox when Master Client assigns weapon
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
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
            
            // FIXED: Set initial weapon direction
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
