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

    // TRICKY SOLUTION: Network variables to force position sync
    private Vector3 networkPosition;
    private bool networkIsFacingRight;
    private float networkSpeed;
    private bool networkIsJumping;
    private bool networkIsDead;

    void Awake()
    {
        view = GetComponent<PhotonView>();
        control = new InputSystem();
        startPos = transform.position;
        
        cameraFollow = Camera.main?.GetComponent<CameraFollow>();
        
        // Initialize network position
        networkPosition = transform.position;
        networkIsFacingRight = isFacingRight;
    }

    private void Start()
    {
        if (!view.IsMine)
        {
            // FIXED: Keep remote players as solid colliders for weapon interactions
            control.Disable();
            activate = false;
            rb.isKinematic = true;
            rb.gravityScale = 0f;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            
            // FIXED: Keep solid collider for weapon pickup interactions
            // GetComponent<Collider2D>().isTrigger = true; // REMOVED - need solid collisions for weapon boxes
            
            Debug.Log($"Remote player {view.Owner.NickName} set to kinematic mode");
            return;
        }
        
        activate = true;
        Debug.Log($"Local player {view.Owner.NickName} physics enabled");
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
            // Local player: Handle input, movement, and animations
            HandleLocalInput();
            UpdateAnimations();
            CheckDeathConditions();
        }
        else
        {
            // TRICKY SOLUTION: Force remote players to always stay at network position
            ForceNetworkPosition();
            UpdateRemoteAnimations();
        }
    }

    void FixedUpdate()
    {
        // TRICKY: Also force position in FixedUpdate for remote players
        if (!view.IsMine)
        {
            ForceNetworkPosition();
        }
    }

    // TRICKY SOLUTION: Force remote players to exact network position every frame
    private void ForceNetworkPosition()
    {
        // Always snap to network position - no interpolation, no physics interference
        transform.position = networkPosition;
        
        // Force zero velocity to prevent any movement
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        
        // Sync facing direction
        if (isFacingRight != networkIsFacingRight)
        {
            Flip();
        }
    }

    private void HandleLocalInput()
    {
        if (isDead || isRespawning || !activate) return;

        // Process input locally for responsive gameplay
        float inputHorizontal = control.Land.Movement.ReadValue<float>();
        bool inputJump = control.Land.Jump.triggered;

        // Process movement immediately on local player
        ProcessPlayerMovement(inputHorizontal, inputJump);
    }

    private void ProcessPlayerMovement(float inputHorizontal, bool inputJump)
    {
        if (isDead || isRespawning || !activate) return;

        // Store previous values for change detection
        bool wasGrounded = IsGrounded();
        
        // Process horizontal movement
        horizontal = inputHorizontal;
        
        if (GetComponent<KnockBackHandler>() && GetComponent<KnockBackHandler>().isKnocking) 
        {
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

        // FIXED: Double jump logic with proper timing
        if (inputJump)
        {
            if (IsGrounded())
            {
                // First jump from ground
                rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
                doubleJump = true; // Enable double jump ability
                jumpRequested = false; // Reset jump request since we're starting fresh
                Debug.Log("First jump executed - double jump enabled");
            }
            else if (doubleJump && !jumpRequested)
            {
                // Second jump (double jump) - only if not already requested this frame
                rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
                doubleJump = false; // Disable double jump until landing
                jumpRequested = true; // Mark that we've used our jump request
                Debug.Log("Double jump executed - no more jumps until landing");
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

        // FIXED: Reset double jump when landing with delay
        if (IsGrounded() && !wasGrounded)
        {
            // Small delay to prevent immediate double jump on landing
            StartCoroutine(ResetDoubleJumpWithDelay());
            Debug.Log("Player landed - resetting double jump");
        }
    }

    // FIXED: Add delay to prevent immediate double jump after landing
    private IEnumerator ResetDoubleJumpWithDelay()
    {
        yield return new WaitForSeconds(0.1f); // Small delay
        doubleJump = false; // Reset double jump availability
        jumpRequested = false; // Reset jump request
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        float animSpeed = Mathf.Abs(currentSpeed);
        bool animIsJumping = !IsGrounded();

        // Set animation parameters safely
        try 
        {
            animator.SetFloat("Speed", animSpeed);
        }
        catch (System.Exception)
        {
            // Speed parameter doesn't exist
        }

        try 
        {
            animator.SetBool("isJumping", animIsJumping);
        }
        catch (System.Exception)
        {
            // isJumping parameter doesn't exist
        }

        // Handle landing sound
        try 
        {
            if (IsGrounded() && animator.GetBool("isJumping"))
            {
                animator.SetBool("isJumping", false);
                if (view.IsMine)
                {
                    SoundManager.PlaySound(SoundManager.Sound.Landing);
                }
            }
        }
        catch (System.Exception)
        {
            // isJumping parameter doesn't exist
        }
    }

    // Update animations for remote players using network data
    private void UpdateRemoteAnimations()
    {
        if (animator == null) return;

        // Use network data for remote player animations
        try 
        {
            animator.SetFloat("Speed", networkSpeed);
        }
        catch (System.Exception)
        {
            // Speed parameter doesn't exist
        }

        try 
        {
            animator.SetBool("isJumping", networkIsJumping);
        }
        catch (System.Exception)
        {
            // isJumping parameter doesn't exist
        }

        // Handle death state visually
        if (networkIsDead && !isDead)
        {
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.3f);
            isDead = true;
        }
        else if (!networkIsDead && isDead)
        {
            GetComponent<SpriteRenderer>().color = Color.white;
            isDead = false;
        }
    }

    // FIXED: Add missing RPC for explosion knockback
    [PunRPC]
    public void ApplyExplosionKnockBack(Vector2 direction, float force, Vector3 explosionCenter)
    {
        if (!view.IsMine) return; // Only apply to own player
        
        KnockBackHandler knockbackHandler = GetComponent<KnockBackHandler>();
        if (knockbackHandler != null)
        {
            // Calculate distance-based force reduction
            float distance = Vector3.Distance(transform.position, explosionCenter);
            float adjustedForce = force / (1 + distance * 0.5f);
            
            Debug.Log($"Applying explosion knockback: Force={adjustedForce}, Distance={distance}");
            knockbackHandler.KnockBack(direction, adjustedForce);
        }
    }

    // TRICKY SOLUTION: IPunObservable sends position + animation data
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Send both position and animation data
            stream.SendNext(transform.position);
            stream.SendNext(Mathf.Abs(currentSpeed));
            stream.SendNext(!IsGrounded());
            stream.SendNext(isFacingRight);
            stream.SendNext(isDead);
        }
        else
        {
            // Receive data and store for forcing position
            networkPosition = (Vector3)stream.ReceiveNext();
            networkSpeed = (float)stream.ReceiveNext();
            networkIsJumping = (bool)stream.ReceiveNext();
            networkIsFacingRight = (bool)stream.ReceiveNext();
            networkIsDead = (bool)stream.ReceiveNext();
        }
    }

    // Keep existing death/respawn methods
    private void CheckDeathConditions()
    {
        if (isDead || isRespawning || !view.IsMine) return;

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
            yield break;
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
        rb.velocity = Vector2.zero;
        
        isDead = false;
        isRespawning = false;
        doubleJump = false;
        jumpRequested = false;
        
        // Reset physics state properly
        if (view.IsMine)
        {
            activate = true;
            rb.isKinematic = false;
            rb.gravityScale = 1f;
        }
        else
        {
            rb.isKinematic = true;
            rb.gravityScale = 0f;
            networkPosition = spawnPosition;
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
        //Debug.Log("Legacy PlayerFell called - now handled by Master Client system");
    }

    private void Respawn()
    {
        //Debug.Log("Legacy Respawn called - now handled by Master Client system");
    }

    private void ResetAll()
    {
        //Debug.Log("Legacy ResetAll called - now handled by Master Client system");
    }
}
