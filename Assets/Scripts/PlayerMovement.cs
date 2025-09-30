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

    // Network variables to force position sync
    private Vector3 networkPosition;
    private bool networkIsFacingRight;
    private float networkSpeed;
    private bool networkIsJumping;
    private bool networkIsDead;

    void Awake()
    {
        view = GetComponent<PhotonView>();
        startPos = transform.position;
        
        control = new InputSystem();
        
        // FIXED: Continuous input reading for ALL players
        control.Land.Movement.performed += ctx =>
        {
            if (!view.IsMine) return;
            
            float inputValue = ctx.ReadValue<float>();
            
            if (view.IsMine && PhotonNetwork.IsMasterClient)
            {
                // Master Client: Process immediately
                horizontal = inputValue;
            }
            else if (view.IsMine && !PhotonNetwork.IsMasterClient)
            {
                // Other players: Send to Master Client AND update locally for responsiveness
                horizontal = inputValue;
                view.RPC("ReceiveMovementInput", RpcTarget.MasterClient, inputValue, Time.time);
            }
        };
        
        control.Land.Movement.canceled += ctx =>
        {
            if (!view.IsMine) return;
            
            if (view.IsMine && PhotonNetwork.IsMasterClient)
            {
                horizontal = 0f;
            }
            else if (view.IsMine && !PhotonNetwork.IsMasterClient)
            {
                horizontal = 0f;
                view.RPC("ReceiveMovementInput", RpcTarget.MasterClient, 0f, Time.time);
            }
        };
        
        // Same fix for jump...
        control.Land.Jump.performed += ctx =>
        {
            if (!view.IsMine || !ctx.ReadValueAsButton()) return;
            
            if (view.IsMine && PhotonNetwork.IsMasterClient)
            {
                jumpRequested = true;
            }
            else if (view.IsMine && !PhotonNetwork.IsMasterClient)
            {
                jumpRequested = true; // Local responsiveness
                view.RPC("ReceiveJumpInput", RpcTarget.MasterClient, Time.time);
            }
        };
        
        cameraFollow = Camera.main?.GetComponent<CameraFollow>();
        networkPosition = transform.position;
        networkIsFacingRight = isFacingRight;
    }

    private void Start()
    {
        if (!view.IsMine)
        {
            control.Disable();
            activate = false;
            rb.isKinematic = true;
            rb.gravityScale = 0f;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            
            Debug.Log($"Remote player {view.Owner.NickName} set to kinematic mode");
            return;
        }
        
        activate = true;
        Debug.Log($"Local player {view.Owner.NickName} physics enabled");
    }

    private void OnEnable()
    {
        if (view != null && view.IsMine)
        {
            control.Enable();
        }
    }

    private void OnDisable()
    {
        if (control != null)
        {
            control.Disable();
        }
    }

    void Update()
    {
        if (view.IsMine)
        {
            UpdateAnimations();
            HandleJump();
            Flip(); // FIXED: Call flip every frame for proper direction tracking
            CheckDeathConditions();
        }
        else
        {
            ForceNetworkPosition();
            UpdateRemoteAnimations();
        }
    }

    void FixedUpdate()
    {
        if (view.IsMine)
        {
            Move();
        }
        else
        {
            ForceNetworkPosition();
        }
    }

    // FIXED: RPC to receive movement input from non-Master players
    [PunRPC]
    void ReceiveMovementInput(float inputHorizontal, float inputTime)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        horizontal = inputHorizontal;
        Debug.Log($"Master Client received movement input for {view.Owner?.NickName}: {inputHorizontal}");
    }

    // FIXED: RPC to receive jump input from non-Master players
    [PunRPC]
    void ReceiveJumpInput(float inputTime)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        jumpRequested = true;
        Debug.Log($"Master Client received jump input for {view.Owner?.NickName}");
    }

    private void Move()
    {
        if (GetComponent<KnockBackHandler>() && GetComponent<KnockBackHandler>().isKnocking) return;
        
        float targetSpeed = horizontal * speed;
        float speedDiff = targetSpeed - currentSpeed;
        float moveAcceleration = (Mathf.Abs(speedDiff) > 0.01f) ? acceleration : deceleration;

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, moveAcceleration * Time.fixedDeltaTime);
        rb.velocity = new Vector2(currentSpeed, rb.velocity.y);
    }

    private void HandleJump()
    {
        if (jumpRequested)
        {
            Jump();
            jumpRequested = false;
        }
    }

    private void Jump()
    {
        Debug.Log("Jump called");
        if (IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
            animator.SetBool("isJumping", true);
            doubleJump = true;
        }
        else if (doubleJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
            doubleJump = false;
        }
    }

    // FIXED: Ensure flip works properly for Master Client
    private void Flip()
    {
        if ((isFacingRight && horizontal < 0f) || (!isFacingRight && horizontal > 0f))
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
            
            // FIXED: Debug log to track flipping
            Debug.Log($"Player {view.Owner?.NickName} flipped to face {(isFacingRight ? "right" : "left")}");
        }
    }

    private void ForceNetworkPosition()
    {
        transform.position = networkPosition;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        
        if (isFacingRight != networkIsFacingRight)
        {
            Vector3 localScale = transform.localScale;
            isFacingRight = networkIsFacingRight;
            localScale.x = isFacingRight ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
            transform.localScale = localScale;
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        animator.SetFloat("Speed", Mathf.Abs(currentSpeed));

        if (IsGrounded())
        {
            if (animator.GetBool("isJumping"))
            {
                animator.SetBool("isJumping", false);
                if (view.IsMine)
                {
                    SoundManager.PlaySound(SoundManager.Sound.Landing);
                }
            }
            doubleJump = false;
        }
    }

    private void UpdateRemoteAnimations()
    {
        if (animator == null) return;

        try 
        {
            animator.SetFloat("Speed", networkSpeed);
            animator.SetBool("isJumping", networkIsJumping);
        }
        catch (System.Exception) { }

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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(Mathf.Abs(currentSpeed));
            stream.SendNext(!IsGrounded());
            stream.SendNext(isFacingRight);
            stream.SendNext(isDead);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkSpeed = (float)stream.ReceiveNext();
            networkIsJumping = (bool)stream.ReceiveNext();
            networkIsFacingRight = (bool)stream.ReceiveNext();
            networkIsDead = (bool)stream.ReceiveNext();
        }
    }

    // Keep all existing death/respawn methods unchanged...
    private void CheckDeathConditions()
    {
        if (isDead || isRespawning || !view.IsMine) return;

        if (Physics2D.OverlapCircle(groundCheck.position, 0.2f, deadLayer))
        {
            RequestDeath("Touched dead layer");
            return;
        }

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

    public bool IsFacingRight()
    {
        return isFacingRight;
    }

    // RPC for explosion knockback
    [PunRPC]
    public void ApplyExplosionKnockBack(Vector2 direction, float force, Vector3 explosionCenter)
    {
        if (!view.IsMine) return;
        
        KnockBackHandler knockbackHandler = GetComponent<KnockBackHandler>();
        if (knockbackHandler != null)
        {
            float distance = Vector3.Distance(transform.position, explosionCenter);
            float adjustedForce = force / (1 + distance * 0.5f);
            
            Debug.Log($"Applying explosion knockback: Force={adjustedForce}, Distance={distance}");
            knockbackHandler.KnockBack(direction, adjustedForce);
        }
    }

    [PunRPC]
    public void ApplyKnockBack(Vector2 direction, float force)
    {
        if (!view.IsMine) return;
        
        KnockBackHandler knockbackHandler = GetComponent<KnockBackHandler>();
        if (knockbackHandler != null)
        {
            Debug.Log($"Applying bullet knockback to {view.Owner?.NickName}: Force={force}");
            knockbackHandler.KnockBack(direction, force);
        }
    }
}
