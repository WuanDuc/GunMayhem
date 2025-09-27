using UnityEngine;using Photon.Pun;using Photon.Realtime;
public class NetworkManager : MonoBehaviourPunCallbacks{
    public static NetworkManager Instance { get; private set; }
    //[Header("Prefabs for Master Client")]
    //public GameObject bulletPoolPrefab;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this; DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); }
    }
    private void Start()
    {        // Initialize systems when connected        if (PhotonNetwork.IsConnected)        {            InitializeMasterClientSystems();        }    }        public override void OnJoinedRoom()    {        Debug.Log($"Joined room as {(PhotonNetwork.IsMasterClient ? "Master Client" : "Client")}");        InitializeMasterClientSystems();    }        public override void OnMasterClientSwitched(Player newMasterClient)    {        Debug.Log($"Master Client switched to: {newMasterClient.NickName}");                if (PhotonNetwork.IsMasterClient)        {            Debug.Log("I am the new Master Client - taking over game authority");            InitializeMasterClientSystems();            TakeOverGameObjects();        }        else        {            Debug.Log("New Master Client assigned - transferring authority");        }    }        private void InitializeMasterClientSystems()    {        if (!PhotonNetwork.IsMasterClient) return;                Debug.Log("Initializing Master Client systems");        
        // Initialize bullet pool if it doesn't exist
        // if (BulletPool.Instance == null && bulletPoolPrefab != null)
        // {
        //     GameObject poolObject = Instantiate(bulletPoolPrefab);
        //     Debug.Log("Bullet pool created by Master Client");
        // }

        // Initialize other Master Client systems here
        InitializeRandomBoxSpawners();
    }
    
    private void InitializeRandomBoxSpawners()
    {
        // Enable random box spawners only on Master Client
        SpawnRandomBox[] spawners = FindObjectsOfType<SpawnRandomBox>();
        foreach (SpawnRandomBox spawner in spawners)
        {
            // The spawner script already handles Master Client checking
            Debug.Log("Random box spawner found and will be handled by Master Client");
        }
    }
    
    private void TakeOverGameObjects()
    {
        // Transfer ownership of game objects when becoming Master Client
        TransferBulletOwnership();
        TransferBoomOwnership();
        // Add other object transfers as needed
    }
    
    private void TransferBulletOwnership()
    {
        // Find all bullets and transfer ownership to new Master Client
        Bullet[] bullets = FindObjectsOfType<Bullet>();
        foreach (Bullet bullet in bullets)
        {
            PhotonView bulletView = bullet.GetComponent<PhotonView>();
            if (bulletView != null && !bulletView.IsMine)
            {
                bulletView.TransferOwnership(PhotonNetwork.LocalPlayer);
                Debug.Log($"Transferred bullet ownership to new Master Client");
            }
        }
    }
    
    private void TransferBoomOwnership()
    {
        // Find all boom objects and transfer ownership to new Master Client
        Boom[] booms = FindObjectsOfType<Boom>();
        foreach (Boom boom in booms)
        {
            PhotonView boomView = boom.GetComponent<PhotonView>();
            if (boomView != null && !boomView.IsMine)
            {
                boomView.TransferOwnership(PhotonNetwork.LocalPlayer);
                Debug.Log($"Transferred boom ownership to new Master Client");
            }
        }
    }
    
    // Public method to check if we can perform Master Client actions
    public bool CanPerformMasterClientAction()
    {
        return !PhotonNetwork.IsConnected || PhotonNetwork.IsMasterClient;
    }
    
    // Method to clean up when leaving room
    public override void OnLeftRoom()
    {
        Debug.Log("Left room - cleaning up Master Client systems");
        
        // // Return all bullets to pool before leaving
        // if (BulletPool.Instance != null)
        // {
        //     BulletPool.Instance.ReturnAllBullets();
        // }
    }
}