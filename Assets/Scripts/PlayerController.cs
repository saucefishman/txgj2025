using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem; // new input system

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // max horizontal speed
    public float xGroundAccel = 10f; // max horizontal acceleration on ground
    public float xAirAccel = 5f; // max horizontal acceleration in air
    public float jumpForce = 14f; // initial jump force
    public float additionalJumpVelocity = 10f; // fixed minimum velocity while jump is held
    public float jumpTime = 0.5f; // max time the jump can be held
    public float preLandBufferTime = 0.1f; // time before landing to allow jump input
    public float baseGravity = 5f; // base gravity scale
    public Vector2 wallJumpForce = new Vector2(10f, 14f); // force applied when wall jumping
    public float foxJumpTimeAllowance = 0.1f; // time after leaving ground or wall to allow jump input
    public float dashAcceleration = 500f; // horizontal acceleration during dash
    public float dashSpeed = 20f; // max horizontal speed during dash
    public float dashDuration = 0.3f; // duration of dash
    public float postJumpWallAttachDelay = 0.2f; // time after jumping to ignore wall attach attempts

    private Direction lastDirection; // Direction player character is facing. Not necessarily aligned with input.

    private bool usedJump = false;
    private Timer jumpHoldTimer; // Tracks for the duration of an extended jump
    private Timer postJumpWallAttachTimer;

    private Timer preLandBufferTimer; // Tracks for jump input before landing

    private Timer groundFoxJumpTimer; // Tracks for jump input after leaving ground
    private Timer wallFoxJumpTimer; // Tracks for jump input after leaving wall

    private Timer dashTimer; // Tracks dash duration
    private bool dashUsed;
    private Direction dashDirection;

    private bool isGrounded = false;

    private bool onWall;
    private Direction lastWallSide;

    private Rigidbody2D rb;
    private Collider2D coll;
    private PlayerControls controls;
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool jumpTapped;
    private bool dashTapped;

    void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Jump.started += ctx =>
        {
            jumpPressed = true;
            preLandBufferTimer.restart();
        };
        controls.Player.Jump.performed += ctx => jumpTapped = true;
        controls.Player.Jump.canceled += ctx => jumpPressed = false;

        controls.Player.Sprint.performed += ctx => dashTapped = true;
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        jumpHoldTimer = new Timer(jumpTime);
        preLandBufferTimer = new Timer(preLandBufferTime);
        groundFoxJumpTimer = new Timer(foxJumpTimeAllowance);
        wallFoxJumpTimer = new Timer(foxJumpTimeAllowance);
        dashTimer = new Timer(dashDuration);
        postJumpWallAttachTimer = new Timer(postJumpWallAttachDelay);
    }

    // TODO: This method is 110 lines long consider killing yourself lmao?
    void Update()
    {
        // Adjust gravity based on state
        if (onWall)
        {
            rb.gravityScale = 0.0f;
        }
        else
        {
            if (!dashTimer.isFinished())
            {
                rb.gravityScale = 0;
            }
            else
            {
                rb.gravityScale = baseGravity;
            }
        }

        // Accept move input if not dashing or on wall
        if (!onWall && dashTimer.isFinished())
        {
            if (moveInput.x != 0)
                lastDirection = moveInput.x > 0 ? Direction.Right : Direction.Left;

            var targetXVelocity = moveInput.x * moveSpeed;
            var accel = isGrounded ? xGroundAccel : xAirAccel;
            var newXVel = Mathf.MoveTowards(rb.linearVelocity.x, targetXVelocity, accel * Time.deltaTime);
            rb.linearVelocity = new Vector2(newXVel, rb.linearVelocity.y);
        }
        // If on wall, check if moving away from wall to leave it
        else if (onWall)
        {
            if ((moveInput.x > 0 && lastWallSide == Direction.Left) ||
                (moveInput.x < 0 && lastWallSide == Direction.Right))
            {
                onLeaveWall();
            }
        }

        if ((jumpTapped || !preLandBufferTimer.isFinished()) && !usedJump)
        {
            if (isGrounded || !groundFoxJumpTimer.isFinished())
            {
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
            else if (onWall || !wallFoxJumpTimer.isFinished())
            {
                var force = wallJumpForce;
                if (lastWallSide == Direction.Right)
                    force.x = -force.x;
                rb.AddForce(force, ForceMode2D.Impulse);
                onLeaveWall();
            }

            usedJump = true;
            jumpHoldTimer.restart();
            jumpTapped = false;
            postJumpWallAttachTimer.restart();
        }

        if (jumpPressed && !onWall)
        {
            if (!jumpHoldTimer.isFinished() && rb.linearVelocity.y < additionalJumpVelocity)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, additionalJumpVelocity);
            }
        }
        else
        {
            jumpHoldTimer.interrupt();
        }

        if (dashTapped)
        {
            if (!dashUsed && dashTimer.isFinished())
            {
                dashDirection = lastDirection;
                dashTimer.restart();
                if (!isGrounded)
                {
                    dashUsed = true;
                }
            }

            dashTapped = false;
        }

        if (!dashTimer.isFinished())
        {
            float targetXVelocity = dashDirection == Direction.Right ? dashSpeed : -dashSpeed;
            float newXVel = Mathf.MoveTowards(rb.linearVelocity.x, targetXVelocity, dashAcceleration * Time.deltaTime);
            rb.linearVelocity = new Vector2(newXVel, 0);
            rb.gravityScale = 0;
        }

        if (dashTimer.hasJustFinished())
        {
            var newXVel = Mathf.Clamp(rb.linearVelocity.x, -moveSpeed, moveSpeed);
            rb.linearVelocity = new Vector2(newXVel, rb.linearVelocity.y);
        }

        Timer.tickAll();
    }

    public void onEnterGround()
    {
        onStayGround();
        dashUsed = false;
    }

    public void onStayGround()
    {
        isGrounded = true;
        jumpHoldTimer.interrupt();
        usedJump = false;
    }

    public void onLeaveGround()
    {
        isGrounded = false;
        groundFoxJumpTimer.restart();
    }

    /**
     * Called when the player first touches a wall.
     *
     * @param wallDirection The direction the wall is in relative to the player.
     * @param wallCollider The collider of the wall the player touched.
     */
    public void onEnterWall(Direction wallDirection, Collider2D wallCollider)
    {
        // There is an intent to stick to the wall if the player is pushing towards it
        // or if the player is dashing towards it.
        if (!(
                (wallDirection == Direction.Right && moveInput.x > 0) ||
                (wallDirection == Direction.Left && moveInput.x < 0) ||
                !dashTimer.isFinished()
            ))
            return;
        if (isGrounded)
            return;
        // ignore wall attach attempts for a short time after jumping
        if (!postJumpWallAttachTimer.isFinished())
            return;

        lastDirection =
            wallDirection == Direction.Left
                ? Direction.Right
                : Direction.Left; // set player direction to face away from wall
        onWall = true;
        lastWallSide = wallDirection; // for the fox jump
        rb.linearVelocity = Vector2.zero;
        dashTimer.interrupt();
        dashUsed = false;
        jumpHoldTimer.interrupt();
        usedJump = false;

        snapToWall(wallCollider.bounds); // set player flush against wall
    }

    void snapToWall(Bounds wallBounds)
    {
        var playerBounds = coll.bounds;
        var pos = rb.position;
        pos.x = pos.x > wallBounds.center.x
            ? wallBounds.max.x + playerBounds.extents.x
            : wallBounds.min.x - playerBounds.extents.x;
        rb.position = pos;
    }

    public void onStayWall(Direction wallDirection, Collider2D wallCollider)
    {
        // sometimes OnTriggerEnter2D doesn't get called, so we call onEnterWall from here too
        // TODO: that's gotta be a bug there's prob a better way to do this
        if (!onWall)
        {
            onEnterWall(wallDirection, wallCollider);
        }
    }

    public void onLeaveWall()
    {
        onWall = false;
        rb.gravityScale = baseGravity;
        wallFoxJumpTimer.restart();
    }
}