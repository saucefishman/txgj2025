using UnityEngine;
using UnityEngine.InputSystem; // new input system

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 14f;
    public float additionalJumpVelocity = 10f; // fixed minimum velocity while jump is held
    public float jumpTime = 0.5f; // max time the jump can be held
    public float preLandBufferTime = 0.1f; // time before landing to allow jump input
    public float baseGravity = 5f; // base gravity scale
    public float gravityOnWall = 1f; // gravity scale when on wall
    public float maxYVelocityOnWall = 2f; // max downward velocity when on wall

    private bool inJump = false;
    private float jumpTimeCounter = 0.0f;

    private float preLandBufferTimer = 0.0f;

    private bool isGrounded = false;

    private bool isOnWall = false;

    private Rigidbody2D rb;
    private PlayerControls controls;
    private Vector2 moveInput;
    private bool jumpPressed;

    void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Jump.started += ctx =>
        {
            jumpPressed = true;
            preLandBufferTimer = preLandBufferTime;
        };
        controls.Player.Jump.canceled += ctx => jumpPressed = false;
        // rb.gravityScale = baseGravity;
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

        if (jumpPressed)
        {
            if (!inJump && isGrounded && preLandBufferTimer > 0)
            {
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                inJump = true;
                jumpTimeCounter = jumpTime;
            }
            else if (inJump && jumpTimeCounter > 0)
            {
                if (rb.linearVelocity.y < additionalJumpVelocity)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, additionalJumpVelocity);
                }
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                if (preLandBufferTimer > 0)
                    preLandBufferTimer -= Time.deltaTime;
            }
        }
        else
        {
            inJump = false;
        }
    }

    public void onEnterGround()
    {
        isGrounded = true;
        inJump = false;
        preLandBufferTimer = 0.0f;
    }
    
    public void onLeaveGround()
    {
        isGrounded = false;
    }

    public void onEnterWall()
    {
        isOnWall = true; 
        rb.gravityScale = gravityOnWall; 
    }
    
    public void onLeaveWall()
    {
        isOnWall = false;
        rb.gravityScale = baseGravity;
    }
}