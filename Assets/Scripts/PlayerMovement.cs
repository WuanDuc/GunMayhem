using Photon.Pun;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PlayerMovement : MonoBehaviourPun
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
    
    // NEW: Death and respawn state management
    [SerializeField] private bool isDead = false;
    [SerializeField] private bool isRespawning = false;
    [SerializeField] private float respawnDelay = 3f;

    void Awake()
    {
        view = GetComponent<PhotonView>();
        control = new InputSystem();
        startPos = transform.position;
        
        // NEW: Find camera follow component
        cameraFollow = Camera.main?.GetComponent<CameraFollow>();
    }

    private void Start()
    {
        if (!view.IsMine)
        {
            // NEW: Only disable input, keep physics for knockback
            control.Disable();
            activate = false;
            return;
        }
        
        // NEW: Enable player for local player
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
            // NEW: Don't process input if dead or respawning
            if (!isDead && !isRespawning && activate)
            {
                // Input handling
                horizontal = control.Land.Movement.ReadValue<float>();
                
                if (control.Land.Jump.triggered)
                {
                    jumpRequested = true;
                }
            }

            // Animation updates (keep running even if dead for visual feedback)
            if (animator != null)
            {
                animator.SetFloat("Speed", Mathf.Abs(currentSpeed));

                if (IsGrounded())
                {
                    if (animator.GetBool("isJumping"))
                    {
                        animator.SetBool("isJumping", false);
                        SoundManager.PlaySound(SoundManager.Sound.Landing);
                    }
                    if (!isDead) doubleJump = false; // Only reset double jump if not dead
                }

                if (jumpRequested && !isDead)
                {
                    Jump();
                    jumpRequested = false; 
                }
            }

            // NEW: Check for death conditions (only for own player)
            if (!isDead && !isRespawning)
            {
                CheckDeathConditions();
            }
        }

        // Flip for all players (visual update)
        if (!isDead) Flip();
    }

    private void FixedUpdate()
    {
        if (view.IsMine)
        {
            // NEW: Don't move if dead or respawning
            if (!isDead && !isRespawning && activate)
            {
                Move();
            }
        }
    }

    // RESTORED: Death condition checking with both layer collision and fall detection
    private void CheckDeathConditions()
    {
        // Check if player is touching dead layer (restored from original)
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

    // NEW: Request death from Master Client
    private void RequestDeath(string reason)
    {
        if (isDead || isRespawning) return;
        
        Debug.Log($"Player {view.Owner.NickName} requesting death: {reason}");
        
        // Send death request to Master Client
        view.RPC("ProcessPlayerDeath", RpcTarget.MasterClient, view.ViewID, reason);
    }

    // NEW: Master Client processes death requests
    [PunRPC]
    void ProcessPlayerDeath(int playerViewID, string reason)
    {
        // Only Master Client processes death
        if (!PhotonNetwork.IsMasterClient) return;
        
        PhotonView targetPlayer = PhotonView.Find(playerViewID);
        if (targetPlayer == null) return;
        
        Debug.Log($"Master Client processing death for {targetPlayer.Owner.NickName}: {reason}");
        
        // Update death count (server-side tracking)
        var props = targetPlayer.Owner.CustomProperties;
        int deaths = props.ContainsKey("deaths") ? (int)props["deaths"] : 0;
        deaths++;
        
        var newProps = new ExitGames.Client.Photon.Hashtable();
        newProps["deaths"] = deaths;
        targetPlayer.Owner.SetCustomProperties(newProps);
        
        // Notify all clients about the death
        targetPlayer.RPC("HandleDeath", RpcTarget.All, reason);
        
        // Start respawn timer
        StartCoroutine(RespawnPlayer(targetPlayer, respawnDelay));
    }

    // NEW: Handle death on all clients
    [PunRPC]
    void HandleDeath(string reason)
    {
        if (isDead) return;
        
        isDead = true;
        activate = false;
        
        Debug.Log($"Player {view.Owner.NickName} died: {reason}");
        
        // Disable player visually and physically
        GetComponent<Collider2D>().enabled = false;
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.3f); // Make transparent
        
        // Destroy current weapon if any
        WeaponHandler weaponHandler = GetComponent<WeaponHandler>();
        if (weaponHandler != null)
        {
            weaponHandler.DestroyCurrentWeapon();
        }
        
        // Stop all movement
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        
        // Reduce spawn count
        if (view.IsMine)
        {
            spawnNum--;
            Debug.Log($"Spawn count reduced to: {spawnNum}");
        }
    }

    // NEW: Respawn coroutine (Master Client only)
    private IEnumerator RespawnPlayer(PhotonView targetPlayer, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (!PhotonNetwork.IsMasterClient || targetPlayer == null) yield break;
        
        // Check if player still has spawns left
        PlayerMovement targetMovement = targetPlayer.GetComponent<PlayerMovement>();
        if (targetMovement != null && targetMovement.spawnNum <= 0)
        {
            Debug.Log($"Player {targetPlayer.Owner.NickName} has no spawns left - not respawning");
            // Could trigger game over logic here
            yield return null;
        }
        
        // Get random spawn point
        PlayerSpawner spawner = FindObjectOfType<PlayerSpawner>();
        if (spawner != null && spawner.spawnerPoints.Length > 0)
        {
            int randomSpawn = Random.Range(0, spawner.spawnerPoints.Length);
            Vector3 spawnPosition = spawner.spawnerPoints[randomSpawn].position;
            spawnPosition.z = 1;
            
            // Notify all clients to respawn this player
            targetPlayer.RPC("HandleRespawn", RpcTarget.All, spawnPosition);
        }
        else
        {
            // Fallback to start position if no spawner found
            targetPlayer.RPC("HandleRespawn", RpcTarget.All, targetMovement.startPos);
        }
    }

    // NEW: Handle respawn on all clients
    [PunRPC]
    void HandleRespawn(Vector3 spawnPosition)
    {
        Debug.Log($"Respawning player {view.Owner.NickName}");
        
        // Reset position
        transform.position = spawnPosition;
        
        // Reset visual and physical state
        GetComponent<Collider2D>().enabled = true;
        GetComponent<SpriteRenderer>().color = Color.white;
        rb.isKinematic = false;
        rb.velocity = Vector2.zero;
        
        // Reset states
        isDead = false;
        isRespawning = false;
        doubleJump = false;
        
        if (view.IsMine)
        {
            activate = true;
        }
        
        Debug.Log($"Player {view.Owner.NickName} respawned successfully");
    }

    // Keep existing movement methods
    private void Move()
    {
        if (GetComponent<KnockBackHandler>().isKnocking) return;
        
        if (horizontal != 0)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, horizontal * speed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.fixedDeltaTime);
        }

        rb.velocity = new Vector2(currentSpeed, rb.velocity.y);
    }

    private void Jump()
    {
        Debug.Log("Jump called");
        if (IsGrounded() || !doubleJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
            if (animator != null) animator.SetBool("isJumping", true);
            
            if (!IsGrounded())
            {
                doubleJump = true;
            }
            else
            {
                doubleJump = false;
            }
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void Flip()
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            Vector3 localScale = transform.localScale;
            isFacingRight = !isFacingRight;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    public bool IsFacingRight()
    {
        return isFacingRight;
    }

    // RESTORED: Legacy methods for compatibility (now handled by new system)
    void PlayerFell()
    {
        // This is now handled by the new Master Client death system
        // Keeping for backwards compatibility but not used
        Debug.Log("Legacy PlayerFell called - now handled by Master Client system");
    }

    private void Respawn()
    {
        // This is now handled by the new Master Client respawn system
        // Keeping for backwards compatibility but not used
        Debug.Log("Legacy Respawn called - now handled by Master Client system");
    }

    private void ResetAll()
    {
        // This is now handled by the new Master Client respawn system
        // Keeping for backwards compatibility but not used
        Debug.Log("Legacy ResetAll called - now handled by Master Client system");
    }
}
