using Photon.Pun;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
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

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask deadLayer;

    [SerializeField] private int spawnNum = 5;

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
    }

    private void Start()
    {
        view = GetComponent<PhotonView>();
        if (!view.IsMine)
        {
            Destroy(this);
        }
    }

    void Update()
    {
        if (view.IsMine)
        {
            Die();

            if (IsGrounded())
            {
                doubleJump = false;
            }

            if (jumpRequested)
            {
                Jump();
                jumpRequested = false; 
            }

            Flip();
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
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
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private void Die()
    {
        if (Physics2D.OverlapCircle(groundCheck.position, 0.2f, deadLayer))
        {
            Respawn();
        }
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
