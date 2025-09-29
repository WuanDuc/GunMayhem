using Photon.Pun;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PlayerMovement : MonoBehaviourPun, IPunObservable
{
    private float horizontal;
    private float speed = 6f;
    private float jumpingPower = 12f;
    private bool isFacingRight = true;
    private bool doubleJump;
    private bool jumpRequested = false;

    private float acceleration = 15f;
    private float deceleration = 5f;
    private float currentSpeed;
    private Vector3 startPos;

    public UnityEvent OnLandEvent;
    public Animator animator;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask deadLayer;

    [SerializeField] private int spawnNum = 20;
    private CameraFollow cameraFollow;
    public bool activate;
    PhotonView view;
    private InputSystem control;
    
    // Death and respawn state management
    [SerializeField] private bool isDead = false;
    [SerializeField] private bool isRespawning = false;
    [SerializeField] private float respawnDelay = 3f;

    // NEW: Network sync variables for animation and movement
    private Vector3 networkPosition;
    private Vector2 networkVelocity;
    private bool networkIsGrounded;
    private bool networkIsFacingRight;
    private float networkSpeed;
    private bool networkIsJumping;
    private bool networkIsDead;

    // NEW: Input state structure for Master Client processing
    private struct PlayerInput
    {
        public float horizontal;
        public bool jump;
        public bool isGrounded;
        public Vector3 position;
        public float timestamp;
    }

    void Awake()
    {
        view = GetComponent<PhotonView>();
        control = new InputSystem();
        startPos = transform.position;
        
        cameraFollow = Camera.main?.GetComponent<CameraFollow>();
        
        // Initialize network variables
        networkPosition = transform.position;
        networkVelocity = Vector2.zero;
        networkIsFacingRight = isFacingRight;
    }

    private void Start()
    {
        if (!view.IsMine)
        {
            // NEW: Non-owned players become kinematic and rely on network updates
            control.Disable();
            activate = false;
            rb.isKinematic = true;
            return;
        }
        
        activate = true;
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

    void Update()
    {
        if (view.IsMine)
        {
            // NEW: Local player sends input to Master Client
            HandleLocalInput();
        }
        else
        {
            // NEW: Remote players interpolate to network state
            InterpolateNetworkState();
        }

        // NEW: Update animations for all players based on network state
        UpdateAnimations();

        // Death condition checking (only for own player)
        if (view.IsMine && !isDead && !isRespawning)
        {
            CheckDeathConditions();
        }
    }

    // NEW: Handle local input and send to Master Client
    private void HandleLocalInput()
    {
        if (isDead || isRespawning || !activate) return;

        // Capture input
        float inputHorizontal = control.Land.Movement.ReadValue<float>();
        bool inputJump = control.Land.Jump.triggered;
        bool grounded = IsGrounded();

        if (PhotonNetwork.IsMasterClient)
        {
            // Master Client processes own input immediately
            ProcessPlayerMovement(inputHorizontal, inputJump, grounded);
        }
        else
        {
            // Send input to Master Client for processing
            view.RPC("ReceivePlayerInput", RpcTarget.MasterClient, 
                inputHorizontal, inputJump, grounded, transform.position, Time.time);
        }
    }

    // NEW: Master Client receives player input
    [PunRPC]
    void ReceivePlayerInput(float inputHorizontal, bool inputJump, bool grounded, Vector3 currentPos, float timestamp)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // Basic anti-cheat: validate position isn't too far from expected
        if (Vector3.Distance(currentPos, transform.position) > 10f)
        {
            Debug.LogWarning($"Player {view.Owner.NickName} position desync detected");
            // Force position correction
            view.RPC("CorrectPosition", view.Owner, transform.position);
            return;
        }

        // Process movement on Master Client
        ProcessPlayerMovement(inputHorizontal, inputJump, grounded);
    }

    // NEW: Master Client processes player movement
    private void ProcessPlayerMovement(float inputHorizontal, bool inputJump, bool grounded)
    {
        if (isDead || isRespawning || !activate) return;

        // Store previous values for change detection
        bool wasGrounded = IsGrounded();
        
        // Process horizontal movement
        horizontal = inputHorizontal;
        
        if (GetComponent<KnockBackHandler>() && GetComponent<KnockBackHandler>().isKnocking) 
        {
            // Don't process movement during knockback
            return;
        }

        if (horizontal != 0)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, horizontal * speed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.fixedDeltaTime);
        }

        // Process jumping
        if (inputJump && (grounded || doubleJump))
        {
            if (grounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
                doubleJump = true;
                jumpRequested = true;
            }
            else if (doubleJump)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
                doubleJump = false;
                jumpRequested = true;
            }
        }

        // Apply movement
        rb.velocity = new Vector2(currentSpeed, rb.velocity.y);

        // Handle flipping
        if (horizontal < 0f && isFacingRight)
        {
            Flip();
        }
        else if (horizontal > 0f && !isFacingRight)
        {
            Flip();
        }

        // Reset double jump when grounded
        if (grounded && !wasGrounded)
        {
            doubleJump = false;
            jumpRequested = false;
        }

        // NEW: Update network state variables for syncing
        networkPosition = transform.position;
        networkVelocity = rb.velocity;
        networkIsGrounded = grounded;
        networkIsFacingRight = isFacingRight;
        networkSpeed = Mathf.Abs(currentSpeed);
        networkIsJumping = !grounded;
        networkIsDead = isDead;
    }

    // NEW: Interpolate remote players to network state
    private void InterpolateNetworkState()
    {
        if (view.IsMine) return;

        // Smoothly move to network position
        transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 15f);
        
        // Apply network velocity for physics consistency
        if (!rb.isKinematic)
        {
            rb.velocity = Vector2.Lerp(rb.velocity, networkVelocity, Time.deltaTime * 10f);
        }

        // Sync facing direction
        if (isFacingRight != networkIsFacingRight)
        {
            Flip();
        }
    }

    // NEW: Update animations based on network state
    private void UpdateAnimations()
    {
        if (animator == null) return;

        // Use network state for remote players, local state for own player
        float animSpeed = view.IsMine ? Mathf.Abs(currentSpeed) : networkSpeed;
        bool animIsGrounded = view.IsMine ? IsGrounded() : networkIsGrounded;
        bool animIsJumping = view.IsMine ? (!IsGrounded() || jumpRequested) : networkIsJumping;
        bool animIsDead = view.IsMine ? isDead : networkIsDead;

        // Set animation parameters
        animator.SetFloat("Speed", animSpeed);
        animator.SetBool("isGrounded", animIsGrounded);
        animator.SetBool("isJumping", animIsJumping);
        animator.SetBool("isDead", animIsDead);

        // Handle landing sound
        if (animIsGrounded && animator.GetBool("isJumping"))
        {
            animator.SetBool("isJumping", false);
            if (view.IsMine) // Only play sound for local player
            {
                SoundManager.PlaySound(SoundManager.Sound.Landing);
            }
        }
    }

    // NEW: Position correction RPC
    [PunRPC]
    void CorrectPosition(Vector3 correctPosition)
    {
        if (!view.IsMine) return;
        
        Debug.Log($"Position corrected by Master Client: {correctPosition}");
        transform.position = correctPosition;
    }

    private void FixedUpdate()
    {
        // NEW: Only Master Client processes physics
        if (!PhotonNetwork.IsMasterClient) return;

        // Master Client handles physics for all players
        // Movement is now handled in ProcessPlayerMovement()
    }

    // NEW: IPunObservable implementation for smooth network sync
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Send data to other clients
            stream.SendNext(transform.position);
            stream.SendNext(rb.velocity);
            stream.SendNext(IsGrounded());
            stream.SendNext(isFacingRight);
            stream.SendNext(Mathf.Abs(currentSpeed));
            stream.SendNext(!IsGrounded() || jumpRequested);
            stream.SendNext(isDead);
        }
        else
        {
            // Receive data from other clients
            networkPosition = (Vector3)stream.ReceiveNext();
            networkVelocity = (Vector2)stream.ReceiveNext();
            networkIsGrounded = (bool)stream.ReceiveNext();
            networkIsFacingRight = (bool)stream.ReceiveNext();
            networkSpeed = (float)stream.ReceiveNext();
            networkIsJumping = (bool)stream.ReceiveNext();
            networkIsDead = (bool)stream.ReceiveNext();
        }
    }

    // Keep existing death/respawn methods as they already use Master Client authority
    private void CheckDeathConditions()
    {
        // Check if player is touching dead layer
        if (Physics2D.OverlapCircle(groundCheck.position, 0.2f, deadLayer))
        {
            RequestDeath("Touched dead layer");
            return;
        }

        // Check if player fell too far down
        if (transform.position.y < -20f)
        {
            RequestDeath("Fell into death zone");
            return;
        }
    }

    private void RequestDeath(string reason)
    {
        if (isDead || isRespawning) return;
        
        Debug.Log($"Player {view.Owner.NickName} requesting death: {reason}");
        view.RPC("ProcessPlayerDeath", RpcTarget.MasterClient, view.ViewID, reason);
    }

    [PunRPC]
    void ProcessPlayerDeath(int playerViewID, string reason)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        PhotonView targetPlayer = PhotonView.Find(playerViewID);
        if (targetPlayer == null) return;
        
        Debug.Log($"Master Client processing death for {targetPlayer.Owner.NickName}: {reason}");
        
        // Update death count
        var props = targetPlayer.Owner.CustomProperties;
        int deaths = props.ContainsKey("deaths") ? (int)props["deaths"] : 0;
        deaths++;
        
        var newProps = new ExitGames.Client.Photon.Hashtable();
        newProps["deaths"] = deaths;
        targetPlayer.Owner.SetCustomProperties(newProps);
        
        targetPlayer.RPC("HandleDeath", RpcTarget.All, reason);
        StartCoroutine(RespawnPlayer(targetPlayer, respawnDelay));
    }

    [PunRPC]
    void HandleDeath(string reason)
    {
        if (isDead) return;
        
        isDead = true;
        activate = false;
        
        Debug.Log($"Player {view.Owner.NickName} died: {reason}");
        
        GetComponent<Collider2D>().enabled = false;
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.3f);
        
        WeaponHandler weaponHandler = GetComponent<WeaponHandler>();
        if (weaponHandler != null)
        {
            weaponHandler.DestroyCurrentWeapon();
        }
        
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        
        if (view.IsMine)
        {
            spawnNum--;
            Debug.Log($"Spawn count reduced to: {spawnNum}");
        }
    }

    private IEnumerator RespawnPlayer(PhotonView targetPlayer, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (!PhotonNetwork.IsMasterClient || targetPlayer == null) yield break;
        
        PlayerMovement targetMovement = targetPlayer.GetComponent<PlayerMovement>();
        if (targetMovement != null && targetMovement.spawnNum <= 0)
        {
            Debug.Log($"Player {targetPlayer.Owner.NickName} has no spawns left - not respawning");
            yield return null;
        }
        
        PlayerSpawner spawner = FindObjectOfType<PlayerSpawner>();
        if (spawner != null && spawner.spawnerPoints.Length > 0)
        {
            int randomSpawn = Random.Range(0, spawner.spawnerPoints.Length);
            Vector3 spawnPosition = spawner.spawnerPoints[randomSpawn].position;
            spawnPosition.z = 1;
            
            targetPlayer.RPC("HandleRespawn", RpcTarget.All, spawnPosition);
        }
        else
        {
            targetPlayer.RPC("HandleRespawn", RpcTarget.All, targetMovement.startPos);
        }
    }

    [PunRPC]
    void HandleRespawn(Vector3 spawnPosition)
    {
        Debug.Log($"Respawning player {view.Owner.NickName}");
        
        transform.position = spawnPosition;
        GetComponent<Collider2D>().enabled = true;
        GetComponent<SpriteRenderer>().color = Color.white;
        rb.isKinematic = false;
        rb.velocity = Vector2.zero;
        
        isDead = false;
        isRespawning = false;
        doubleJump = false;
        
        if (view.IsMine)
        {
            activate = true;
        }
        
        Debug.Log($"Player {view.Owner.NickName} respawned successfully");
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void Flip()
    {
        Vector3 localScale = transform.localScale;
        isFacingRight = !isFacingRight;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    public bool IsFacingRight()
    {
        return isFacingRight;
    }

    // Legacy methods for compatibility
    void PlayerFell()
    {
        Debug.Log("Legacy PlayerFell called - now handled by Master Client system");
    }

    private void Respawn()
    {
        Debug.Log("Legacy Respawn called - now handled by Master Client system");
    }

    private void ResetAll()
    {
        Debug.Log("Legacy ResetAll called - now handled by Master Client system");
    }
}
