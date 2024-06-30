using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class PlayerMovement : MonoBehaviourPun
{
    private float horizontal;
    private float speed = 6f;
    private float jumpingPower = 12f;
    private bool isFacingRight = true;

    private bool doubleJump;

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

    private void Awake()
    {
        startPos = transform.position;
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
            HandleInput();
            HandleJump();

            animator.SetFloat("Speed", Mathf.Abs(currentSpeed));

            if (IsGrounded())
            {
                if (animator.GetBool("isJumping"))
                {
                    animator.SetBool("isJumping", false);
                }
                doubleJump = false;
            }
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
                animator.SetBool("isJumping", true);

                if (!IsGrounded())
                {
                    doubleJump = true;
                }
            }
        }
    }

    private void Move()
    {
        float targetSpeed = horizontal * speed;
        float speedDiff = targetSpeed - currentSpeed;
        float moveAcceleration = (Mathf.Abs(speedDiff) > 0.01f) ? acceleration : deceleration;

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, moveAcceleration * Time.fixedDeltaTime);

        rb.velocity = new Vector2(currentSpeed, rb.velocity.y);
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
            PlayerFell();
            Respawn();
        }
    }

    [PunRPC]
    void PlayerFell()
    {
        Photon.Realtime.Player player = PhotonNetwork.LocalPlayer;
        ExitGames.Client.Photon.Hashtable playerProperties = player.CustomProperties;
        playerProperties["deaths"] = (int)playerProperties["deaths"] + 1;
        player.SetCustomProperties(playerProperties);
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (view.IsMine)
        {
            view.RPC("KnockbackRPC", RpcTarget.All, direction, force);
        }
    }

    [PunRPC]
    void KnockbackRPC(Vector2 direction, float force)
    {
        Vector2 impulse = direction.normalized * force;
        rb.AddForce(impulse, ForceMode2D.Impulse);
    }

    private void Respawn()
    {
        if (spawnNum < 0)
        {
            Destroy(gameObject);
            return;
        }
        ResetAll();
    }

    private void ResetAll()
    {
        transform.position = startPos;
        if (transform.Find("WeaponManager").childCount > 0)
        {
            Destroy(transform.Find("WeaponManager").GetChild(0).gameObject);
        }
        spawnNum--;
    }
}
