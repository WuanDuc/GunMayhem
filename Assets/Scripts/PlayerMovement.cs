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
    
    // Thêm biến để track death state
    private bool isDead = false;
    private bool isRespawning = false;

    void Awake()
    {
        startPos = transform.position;
        control = new InputSystem();
        control.Enable();
        control.Land.Movement.performed += ctx =>
        {
            horizontal = ctx.ReadValue<float>();
        };
        control.Land.Jump.performed += ctx =>
        {
            if (ctx.ReadValueAsButton())
            {
                jumpRequested = true;
            }
        };
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
        view = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (!view.IsMine)
        {
            Destroy(this);
        }
    }

    private void Update()
    {
        if (view.IsMine && !isDead && !isRespawning)
        {
            animator.SetFloat("Speed", Mathf.Abs(currentSpeed));

            if (IsGrounded())
            {
                if (animator.GetBool("isJumping"))
                {
                    animator.SetBool("isJumping", false);
                    SoundManager.PlaySound(SoundManager.Sound.Landing);
                }
                doubleJump = false;
            }

            if (jumpRequested)
            {
                Jump();
                jumpRequested = false; 
            }

            Flip();
            
            // Chỉ check death khi chưa chết và chưa respawning
            CheckOwnDeath();
        }
        else if (!view.IsMine)
        {
            Flip(); // Other players vẫn cần flip
        }
    }

    private void FixedUpdate()
    {
        if (view.IsMine && !isDead && !isRespawning)
        {
            Move();
        }
    }

    private void CheckOwnDeath()
    {
        // Chỉ check death khi player chưa chết
        if (!isDead && Physics2D.OverlapCircle(groundCheck.position, 0.2f, deadLayer))
        {
            isDead = true; // Prevent multiple death calls
            
            // Gửi death request lên Master Client
            view.RPC("RequestPlayerDeath", RpcTarget.MasterClient, view.ViewID);
        }
    }

    [PunRPC]
    void RequestPlayerDeath(int playerViewID)
    {
        // Chỉ Master Client xử lý death request
        if (!PhotonNetwork.IsMasterClient) return;
        
        PhotonView playerPV = PhotonView.Find(playerViewID);
        if (playerPV != null)
        {
            // Check if this player is already dead to prevent spam
            PlayerMovement playerMovement = playerPV.GetComponent<PlayerMovement>();
            if (playerMovement != null && !playerMovement.isDead)
            {
                // Master Client thông báo cho tất cả về death
                photonView.RPC("OnPlayerDied", RpcTarget.All, playerViewID);
            }
        }
    }

    [PunRPC]
    void OnPlayerDied(int playerViewID)
    {
        PhotonView playerPV = PhotonView.Find(playerViewID);
        if (playerPV != null && playerPV.IsMine && !isRespawning)
        {
            isRespawning = true; // Prevent multiple respawn calls
            
            // Update death count trước khi respawn
            UpdateDeathCount();
            
            // Destroy weapon trước khi respawn
            DestroyCurrentWeapon();
            
            // Respawn player
            StartCoroutine(RespawnCoroutine());
        }
    }
    
    void DestroyCurrentWeapon()
    {
        WeaponHandler weaponHandler = GetComponent<WeaponHandler>();
        if (weaponHandler != null)
        {
            weaponHandler.DestroyCurrentWeapon();
        }
    }
    
    void UpdateDeathCount()
    {
        var props = PhotonNetwork.LocalPlayer.CustomProperties;
        props["deaths"] = (int)(props["deaths"] ?? 0) + 1;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    IEnumerator RespawnCoroutine()
    {
        // Disable player hoàn toàn
        DisablePlayer();
        
        yield return new WaitForSeconds(2f);
        
        // Respawn using PlayerSpawner
        PlayerSpawner spawner = FindObjectOfType<PlayerSpawner>();
        if (spawner != null && spawner.spawnerPoints != null && spawner.spawnerPoints.Length > 0)
        {
            // Get random spawn point từ PlayerSpawner
            Transform[] spawnPoints = spawner.spawnerPoints;
            int randomSpawn = Random.Range(0, spawnPoints.Length);
            Transform selectedSpawnPoint = spawnPoints[randomSpawn];
            
            // Reset position theo spawn point
            Vector3 spawnPosition = selectedSpawnPoint.position;
            spawnPosition.z = transform.position.z;
            
            // Teleport player ngay lập tức
            rb.velocity = Vector2.zero;
            transform.position = spawnPosition;
            
            Debug.Log($"Player respawned at: {selectedSpawnPoint.name} - Position: {spawnPosition}");
        }
        else
        {
            Debug.LogError("PlayerSpawner or spawn points not found! Using start position.");
            rb.velocity = Vector2.zero;
            transform.position = startPos;
        }
        
        // Re-enable player
        EnablePlayer();
        
        // Reset death flags
        isDead = false;
        isRespawning = false;
    }
    
    void DisablePlayer()
    {
        // Disable movement
        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        
        // Disable visuals
        GetComponent<Collider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
        
        // Disable input (đã có check isDead trong Update)
        
        // Disable camera follow tạm thời
        if (cameraFollow != null && cameraFollow.target == transform)
        {
            // Không disable camera, chỉ dừng follow
        }
        
        ResetPlayerState();
    }
    
    void EnablePlayer()
    {
        // Re-enable physics
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.velocity = Vector2.zero;
        
        // Re-enable components
        GetComponent<Collider2D>().enabled = true;
        GetComponent<SpriteRenderer>().enabled = true;
        
        // Reset state
        ResetPlayerState();
    }
    
    void ResetPlayerState()
    {
        // Reset movement variables
        currentSpeed = 0f;
        horizontal = 0f;
        doubleJump = false;
        jumpRequested = false;
        
        // Reset animations
        animator.SetFloat("Speed", 0f);
        animator.SetBool("isJumping", false);
        
        // Reset knockback if exists
        KnockBackHandler knockback = GetComponent<KnockBackHandler>();
        if (knockback != null)
        {
            knockback.isKnocking = false;
        }
    }

    private void Move()
    {
        if (GetComponent<KnockBackHandler>().isKnocking) return;
        
        float targetSpeed = horizontal * speed;
        float speedDiff = targetSpeed - currentSpeed;
        float moveAcceleration = (Mathf.Abs(speedDiff) > 0.01f) ? acceleration : deceleration;

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, moveAcceleration * Time.fixedDeltaTime);

        rb.velocity = new Vector2(currentSpeed, rb.velocity.y);
    }

    private void Jump()
    {
        if (IsGrounded() || !doubleJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
            animator.SetBool("isJumping", true);
            if (!IsGrounded())
            {
                doubleJump = true;
            }
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void Flip()
    {
        if ((isFacingRight && horizontal < 0f) || (!isFacingRight && horizontal > 0f))
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    public bool IsFacingRight()
    {
        return isFacingRight;
    }
    
    // Public methods để check state
    public bool IsDead()
    {
        return isDead;
    }
    
    public bool IsRespawning()
    {
        return isRespawning;
    }
}
