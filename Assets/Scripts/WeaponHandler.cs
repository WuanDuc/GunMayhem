using UnityEngine;
using Photon.Pun;
using Photon.Pun.Demo.Asteroids;

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
    private InputSystem control;
    
    private void Awake()
    {
        weaponManager = transform.Find("WeaponManager");
        view = GetComponent<PhotonView>();
        control = new InputSystem();
        
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
            // NEW: Enhanced weapon input forwarding to Master Client
            HandleWeaponInput();
        }

        // NEW: Master Client handles all weapon timers
        if (PhotonNetwork.IsMasterClient && boomTimer > 0)
        {
            boomTimer -= Time.deltaTime;
        }
    }

    // NEW: Handle weapon input and forward to Master Client
    private void HandleWeaponInput()
    {
        // FIXED: Use correct input action names from InputSystem
        bool firePressed = control.Land.Shoot.triggered; // Changed from Fire to Shoot
        bool boomPressed = control.Land.ThrowBoom.triggered; // Use ThrowBoom action

        if (firePressed && weapon != null)
        {
            if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
            {
                // Send fire input to Master Client
                view.RPC("ReceiveFireInput", RpcTarget.MasterClient, Time.time);
            }
            else if (PhotonNetwork.IsMasterClient)
            {
                // Master Client processes own fire input
                ProcessFireInput(Time.time);
            }
        }

        if (boomPressed && boomNum > 0)
        {
            if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
            {
                // Send boom input to Master Client
                view.RPC("ReceiveBoomInput", RpcTarget.MasterClient, transform.position);
            }
            else if (PhotonNetwork.IsMasterClient)
            {
                // Master Client processes own boom input
                ProcessBoomInput(transform.position);
            }
        }

        // OLD CODE - commented out
        /*
        // NEW: Use keyboard input for boom since InputSystem doesn't have Boom action
        bool boomPressed = Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space);
        
        if (view.IsMine)
        {
            if (control.Land.Fire.triggered && weapon != null)  // ERROR: Fire doesn't exist
            {
                if (Time.time > nextTimeToFire)
                {
                    weapon.GetComponent<Weapon>().Shoot(Vector2.right);
                    nextTimeToFire = Time.time + 1f / weapon.GetComponent<Weapon>().fireRate;
                }
            }

            // NEW: Master Client authority for boom spawning
            if (control.Land.Boom.triggered && boomNum > 0)  // ERROR: Boom doesn't exist
            {
                boomTimer = boomCountDown;
                boomNum--;
                
                if (PhotonNetwork.IsConnected)
                {
                    // Send boom request to Master Client
                    view.RPC("RequestBoomSpawn", RpcTarget.MasterClient, transform.position);
                }
                else
                {
                    // Offline mode - direct instantiation
                    Instantiate(boomPrefab, transform.position, Quaternion.identity);
                }
            }

            if (boomTimer > 0)
            {
                boomTimer -= Time.deltaTime;
            }
        }
        */
    }

    // NEW: Master Client receives fire input
    [PunRPC]
    void ReceiveFireInput(float inputTime)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        // Validate firing rate on server-side
        ProcessFireInput(inputTime);
    }

    // NEW: Master Client processes fire input
    private void ProcessFireInput(float inputTime)
    {
        if (weapon == null) return;
        
        // Server-side fire rate validation
        if (inputTime > nextTimeToFire)
        {
            // Process shooting
            weapon.GetComponent<Weapon>().Shoot(Vector2.right);
            nextTimeToFire = inputTime + 1f / weapon.GetComponent<Weapon>().fireRate;
            
            Debug.Log($"Master Client processed fire for player: {view.Owner?.NickName}");
        }
        else
        {
            Debug.Log($"Fire input rejected - too fast from player: {view.Owner?.NickName}");
        }
    }

    // NEW: Master Client receives boom input
    [PunRPC]
    void ReceiveBoomInput(Vector3 spawnPosition)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        ProcessBoomInput(spawnPosition);
    }

    // NEW: Master Client processes boom input
    private void ProcessBoomInput(Vector3 spawnPosition)
    {
        if (boomNum <= 0 || boomTimer > 0) return;
        
        boomTimer = boomCountDown;
        boomNum--;
        
        // Spawn boom on Master Client
        GameObject boom = PhotonNetwork.Instantiate(boomPrefab.name, spawnPosition, Quaternion.identity);
        
        // Sync boom count to player
        view.RPC("SyncBoomCount", view.Owner, boomNum, boomTimer);
        
        Debug.Log($"Master Client spawned boom for player: {view.Owner?.NickName}");
    }

    // NEW: Sync boom state back to player
    [PunRPC]
    void SyncBoomCount(int newBoomNum, float newBoomTimer)
    {
        if (!view.IsMine) return;
        
        boomNum = newBoomNum;
        boomTimer = newBoomTimer;
        
        Debug.Log($"Boom count synced: {boomNum} remaining");
    }

    // Keep existing methods with Master Client authority
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
        }
        else
        {
            weapon = Instantiate(newWeapon, weaponManager);
        }
        
        Debug.Log($"Equipped new weapon: {newWeapon.name} for player: {view.Owner?.NickName}");
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
    void EquipNetworkWeapon(int weaponViewID)
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
            
            Debug.Log($"Network weapon equipped: {weapon.name} for player: {view.Owner?.NickName}");
        }
        else
        {
            Debug.LogError($"Could not find weapon with ViewID: {weaponViewID}");
        }
    }

    // OLD RPC - now unused but kept for compatibility
    [PunRPC]
    void RequestBoomSpawn(Vector3 spawnPosition)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        Debug.Log("Master Client spawning boom at position: " + spawnPosition);
        GameObject boom = PhotonNetwork.Instantiate(boomPrefab.name, spawnPosition, Quaternion.identity);
        Debug.Log("Boom spawned by Master Client");
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
