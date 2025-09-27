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
        if (view.IsMine)
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
            
            // Check death only on Master Client
            if (PhotonNetwork.IsMasterClient)
            {
                CheckPlayerDeath();
            }
        }
        Flip();
    }

    private void FixedUpdate()
    {
        if (view.IsMine)
        {
            Move();
        }
    }

    private void HandleInput()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (IsGrounded() || !doubleJump)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpingPower);

                if (!IsGrounded())
                {
                    doubleJump = true;
                }
            }
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
        Debug.Log("Jump called");
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

    private void CheckPlayerDeath()
    {
        // Check all players for death conditions
        PlayerMovement[] allPlayers = FindObjectsOfType<PlayerMovement>();
        foreach (var player in allPlayers)
        {
            if (Physics2D.OverlapCircle(player.groundCheck.position, 0.2f, deadLayer))
            {
                // Player died, notify all clients
                photonView.RPC("OnPlayerDied", RpcTarget.All, player.photonView.ViewID);
            }
        }
    }

    [PunRPC]
    void OnPlayerDied(int playerViewID)
    {
        PhotonView playerPV = PhotonView.Find(playerViewID);
        if (playerPV != null && playerPV.IsMine)
        {
            // Only the dead player handles their own death
            StartCoroutine(RespawnCoroutine());
        }
    }

    IEnumerator RespawnCoroutine()
    {
        // Disable player temporarily
        gameObject.SetActive(false);
        
        yield return new WaitForSeconds(2f);
        
        // Respawn at random position
        Transform[] spawnPoints = FindObjectOfType<PlayerSpawner>().spawnerPoints;
        int randomSpawn = Random.Range(0, spawnPoints.Length);
        transform.position = spawnPoints[randomSpawn].position;
        
        gameObject.SetActive(true);
    }
}
