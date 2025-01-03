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
            //HandleInput();
            //HandleJump();

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
        }
        Flip();
        Die();
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

    private void Die()
    {
        if (Physics2D.OverlapCircle(groundCheck.position, 0.2f, deadLayer))
        {
            //view.RPC("PlayerFell", RpcTarget.AllBuffered);
            PlayerFell();
            //view.RPC("Respawn", RpcTarget.AllBuffered);
            Respawn();
        }
    }
    //[PunRPC]
    void PlayerFell()
    {
        Photon.Realtime.Player player = PhotonNetwork.LocalPlayer;
        ExitGames.Client.Photon.Hashtable playerProperties = player.CustomProperties;

        if (playerProperties.ContainsKey("deaths"))
        {
            playerProperties["deaths"] = (int)playerProperties["deaths"] + 1;
        }
        else
        {
            playerProperties["deaths"] = 1;
        }

        player.SetCustomProperties(playerProperties);
    }

    //[PunRPC]
    private void Respawn()
    {
        if (view.IsMine)
        {
            if (spawnNum < 0)
            {
                PhotonNetwork.Destroy(gameObject);
                return;
            }
            //view.RPC("ResetAll", RpcTarget.AllBuffered);
            ResetAll();
        }
    }
    //void PlayerFell()
    //{
    //    Photon.Realtime.Player player = PhotonNetwork.LocalPlayer;
    //    ExitGames.Client.Photon.Hashtable playerProperties = player.CustomProperties;
    //    playerProperties["deaths"] = (int)playerProperties["deaths"] + 1;
    //    player.SetCustomProperties(playerProperties);
    //}

    //private void Respawn()
    //{
    //    if (spawnNum < 0)
    //    {
    //        PhotonNetwork.Destroy(gameObject);
    //        return;
    //    }
    //    ResetAll();
    //}

    //[PunRPC]
    private void ResetAll()
    {
        transform.position = startPos;
        if (transform.Find("WeaponManager").childCount > 0)
        {
            PhotonNetwork.Destroy(transform.Find("WeaponManager").GetChild(0).gameObject);
        }
        spawnNum--;
    }
}
