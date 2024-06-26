﻿using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float horizontal;
    private float speed = 6f;
    private float jumpingPower = 12f;
    private bool isFacingRight = true;
    // public float fallMultiplier = 2.5f;
    // public float lowJumpMultiplier = 2f;
    private bool doulbeJump;

    private float acceleration = 15f;
    private float deceleration = 10f;
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
            doulbeJump = false;
        }


        if (Input.GetButtonDown("Jump"))
        {
            if (IsGrounded() || doulbeJump)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpingPower);

                doulbeJump = !doulbeJump;
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
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, (horizontal != 0 ? acceleration : deceleration) * Time.fixedDeltaTime);
        rb.velocity = new Vector2(currentSpeed, rb.velocity.y);
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
}