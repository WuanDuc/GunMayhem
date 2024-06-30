using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
public class PlayerMovement : MonoBehaviour
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
    void Awake()
    {
        //rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
    }
    private void Start()
    {
        //this.activate = false;
        view = GetComponent<PhotonView>();
        if (!view.IsMine)
        {
            Destroy(this);
        }
    }
    public void Activate()
    {
        this.activate = true;
    }
    public void DeActivate()
    {
        this.activate = false;
    }
    void Update()
    {
        animator.SetFloat("Speed", Mathf.Abs(currentSpeed));

        if (view.IsMine)
            Die();
        {
            horizontal = Input.GetAxisRaw("Horizontal");

            if (IsGrounded() && !Input.GetButton("Jump"))
            {
                animator.SetBool("isJumping", false);
                doubleJump = false;
            }


            if (Input.GetButtonDown("Jump"))
            {
                
                if (IsGrounded() || doubleJump)
                {
                    rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
                    animator.SetBool("isJumping", true);
                        doubleJump = true;
                    if (!IsGrounded())
                    {
                        doubleJump = false;
                        rb.velocity = new Vector2(rb.velocity.x, jumpingPower * 4 / 5);
                    }
                }
            }
            Flip();
        }
    }

    private void FixedUpdate()
    {
        //rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
        Move();
    }
    private void Move()
    {
        float targetSpeed = horizontal * speed;
        float speedDiff = targetSpeed - currentSpeed;
        float moveAcceleration = (Mathf.Abs(speedDiff) > 0.01f) ? acceleration : deceleration;

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, moveAcceleration * Time.fixedDeltaTime);

        //vertical velocity the same while apply horizontal movement
        rb.velocity = new Vector2(currentSpeed, rb.velocity.y);
    }

    private bool IsGrounded()
    {
        bool grounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        return grounded;
    }

    private void Flip()
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
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
        //increase death count for the local player
        Photon.Realtime.Player player = PhotonNetwork.LocalPlayer;
        ExitGames.Client.Photon.Hashtable playerProperties = player.CustomProperties;
        playerProperties["deaths"] = (int)playerProperties["deaths"] + 1;
        player.SetCustomProperties(playerProperties);

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