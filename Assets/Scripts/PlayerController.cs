using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem; // new input system

public class PlayerController : MonoBehaviour
{
    private static string HEALTH_PACK_TAG = "HealthPack";
    private static string FROZEN_FIELD_TAG = "FrozenField";

    public float moveSpeed = 5f; // max horizontal speed
    public float xGroundAccel = 10f; // max horizontal acceleration on ground
    public float xAirAccel = 5f; // max horizontal acceleration in air

    public float baseGravity = 5f; // base gravity scale
    public float additionalJumpVelocity = 10f; // fixed minimum velocity while jump is held
    public float jumpTime = 0.5f; // max time the jump can be held
    public float preLandBufferTime = 0.1f; // time before landing to allow jump input
    public float foxJumpTimeAllowance = 0.1f; // time after leaving ground or wall to allow jump input
    public Vector2 wallJumpForce = new Vector2(10f, 14f); // force applied when wall jumping
    public float jumpForce = 14f; // initial jump force
    public float postJumpWallAttachDelay = 0.2f; // time after jumping to ignore wall attach attempts

    public float dashAcceleration = 500f; // horizontal acceleration during dash
    public float dashSpeed = 20f; // max horizontal speed during dash
    public float dashDuration = 0.3f; // duration of dash
    public float dashCooldown = 0.5f; // Time after dashing before dash can be used again

    public float baseLifetime = 60.0f;

    public OverlayController overlayController; // reference to overlay UI's controller

    public Transform respawnPoint;

    public Transform cameraTarget;

    private Direction lastDirection; // Direction player character is facing. Not necessarily aligned with input.

    private bool usedJump = false;
    private Timer jumpHoldTimer; // Tracks for the duration of an extended jump
    private Timer postJumpWallAttachTimer;

    private Timer preLandBufferTimer; // Tracks for jump input before landing

    private Timer groundFoxJumpTimer; // Tracks for jump input after leaving ground
    private Timer wallFoxJumpTimer; // Tracks for jump input after leaving wall

    private Timer dashTimer; // Tracks dash duration
    private Timer dashCooldownTimer;
    private bool dashUsed;
    private Direction dashDirection;

    private bool isGrounded = false;

    private bool onWall;
    private Direction lastWallSide;

    private Timer lifeTimer;
    private bool dying;

    private DialogueInterface
        targetDialogueInterface; // doesn't work when there's multiple next to each other but who cares

    private bool speaking = false;

    private Rigidbody2D rb;
    private Collider2D coll;
    private PlayerControls controls;
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool jumpTapped;
    private bool dashTapped;
    private bool interactTapped;

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

        controls.Player.Interact.started += ctx => interactTapped = true;

        jumpHoldTimer = new Timer(jumpTime);
        preLandBufferTimer = new Timer(preLandBufferTime);
        groundFoxJumpTimer = new Timer(foxJumpTimeAllowance);
        wallFoxJumpTimer = new Timer(foxJumpTimeAllowance);
        dashTimer = new Timer(dashDuration);
        postJumpWallAttachTimer = new Timer(postJumpWallAttachDelay);
        dashCooldownTimer = new Timer(dashCooldown);
        lifeTimer = new Timer(baseLifetime, addToRegistry: false);

        if (overlayController == null)
        {
            Debug.LogWarning("OverlayController not set on PlayerController");
        }

        if (respawnPoint == null)
        {
            Debug.LogWarning("Respawn point not set on PlayerController");
        }

        targetDialogueInterface = null;
        speaking = false;
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        lifeTimer.restart();
        dying = true;
    }

    // TODO: This method is 110 lines long consider killing yourself lmao?
    void Update()
    {
        if (speaking && targetDialogueInterface != null)
        {
            cameraTarget.position = Vector3.Lerp(
                transform.position,
                targetDialogueInterface.gameObject.transform.position,
                0.5f
            );
        }
        else
        {
            cameraTarget.position = transform.position;
        }

        if (interactTapped && targetDialogueInterface != null)
        {
            if (targetDialogueInterface.isInDialogue())
            {
                var successfullyAdvanced = targetDialogueInterface.advanceDialogue();
                if (!successfullyAdvanced) speaking = false;
            }
            else
            {
                targetDialogueInterface.startDialogue();
                speaking = true;
                rb.linearVelocity = Vector2.zero;
            }
        }

        interactTapped = false;

        if (speaking)
            return;

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
            var canJumpFromGround = isGrounded || !groundFoxJumpTimer.isFinished();
            var canJumpFromWall = onWall || !wallFoxJumpTimer.isFinished();

            if (canJumpFromGround || canJumpFromWall)
            {
                usedJump = true;
                jumpHoldTimer.restart();
                postJumpWallAttachTimer.restart();
            }

            if (canJumpFromGround)
            {
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
            else if (canJumpFromWall)
            {
                var force = wallJumpForce;
                if (lastWallSide == Direction.Right)
                    force.x = -force.x;
                rb.AddForce(force, ForceMode2D.Impulse);
                onLeaveWall();
            }
        }

        jumpTapped = false;

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
            if (!dashUsed && dashTimer.isFinished() && dashCooldownTimer.isFinished())
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
            dashCooldownTimer.restart();
        }

        overlayController.setHealth(lifeTimer.getTimeRemaining() / baseLifetime);

        if (dying)
        {
            lifeTimer.tick();
            if (lifeTimer.isFinished())
            {
                rb.position = respawnPoint.position;
                rb.linearVelocity = Vector2.zero;
                lifeTimer.restart();
            }
        }

        Timer.tickRegistered();
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

    /// <summary>
    /// Called when the player first touches a wall.
    /// </summary>
    /// <param name="wallDirection">The direction the wall is in relative to the player.</param>
    /// <param name="wallCollider">The collider of the wall the player touched.</param>
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

        // snapToWall(wallCollider.bounds); // set player flush against wall
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(HEALTH_PACK_TAG))
        {
            var hp = other.GetComponent<HealthPack>();
            if (hp == null)
            {
                Debug.LogWarning("HealthPack object not found");
                return;
            }

            var amount = hp.consumeAndGetAmount();
            lifeTimer.setTimeRemaining(Math.Min(lifeTimer.getTimeRemaining() + amount, baseLifetime));
        }
        else if (other.CompareTag(FROZEN_FIELD_TAG))
        {
            dying = false;
        }
        else if (other.CompareTag("NPC"))
        {
            var otherDialogueInterface = other.GetComponent<DialogueInterface>();
            targetDialogueInterface = otherDialogueInterface;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(FROZEN_FIELD_TAG))
        {
            dying = true;
        }
        else if (other.CompareTag("NPC"))
        {
            targetDialogueInterface = null;
        }
    }
}