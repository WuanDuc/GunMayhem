using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float horizontal;
    private float speed = 6f;
    private float jumpingPower = 11f;
    private bool isFacingRight = true;
    // public float fallMultiplier = 2.5f;
    // public float lowJumpMultiplier = 2f;
    private bool doubleJump;

    private float acceleration = 15f;
    private float deceleration = 5f;
    private float currentSpeed;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    // void Awake()
    // {
    //     rb = GetComponent<Rigidbody2D>();
    // }

    void Update()
    {
        // if (rb.velocity.y < 0)
        // {
        //     rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        // } else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
        // {
        //     rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        // }

        horizontal = Input.GetAxisRaw("Horizontal");

        if (IsGrounded() && !Input.GetButton("Jump"))
        {
            doubleJump = false;
        }


        if (Input.GetButtonDown("Jump"))
        {
            if (IsGrounded() || !doubleJump)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
                if (!IsGrounded())
                {
                    doubleJump = true;
                    rb.velocity = new Vector2(rb.velocity.x, jumpingPower * 4 / 5);
                }
            }
        }


        Flip();
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

        if (grounded)
        {
            Debug.Log("Grounded");
        }
        else
        {
            Debug.Log("Not Grounded");
        }

        //draw ground check in the Scene view
        Debug.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * 0.2f, grounded ? Color.green : Color.red);

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
}