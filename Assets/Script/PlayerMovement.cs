using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed  = 9f;
    public float jumpForce = 12f;

    [Header("Jump Feel")]
    public float fallMultiplier    = 2.5f;
    public float lowJumpMultiplier = 2f;
    public float jumpBufferTime    = 0.15f;
    public float coyoteTime        = 0.1f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.25f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool wasGrounded;
    private float moveInput;
    private bool isRunning;
    private PlayerState playerState;

    private float jumpBufferCounter;
    private float coyoteTimeCounter;

    private float runDustTimer = 0f;
    private const float RUN_DUST_INTERVAL = 0.18f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerState = GetComponent<PlayerState>();
    }

    void Update()
    {
        wasGrounded = isGrounded;
        isGrounded  = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Land event
        if (!wasGrounded && isGrounded && rb.linearVelocity.y <= 0.1f)
            if (EffectsManager.Instance != null)
                EffectsManager.Instance.PlayerLand(groundCheck.position);

        moveInput = Input.GetAxisRaw("Horizontal");
        isRunning = Input.GetKey(KeyCode.LeftShift);

        coyoteTimeCounter = isGrounded ? coyoteTime : coyoteTimeCounter - Time.deltaTime;

        if (Input.GetButtonDown("Jump")) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
            if (EffectsManager.Instance != null)
                EffectsManager.Instance.PlayerJump(groundCheck.position);
        }

        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            coyoteTimeCounter = 0f;
        }

        // Run dust
        if (isGrounded && Mathf.Abs(moveInput) > 0.1f && isRunning)
        {
            runDustTimer -= Time.deltaTime;
            if (runDustTimer <= 0f)
            {
                runDustTimer = RUN_DUST_INTERVAL;
                if (EffectsManager.Instance != null)
                    EffectsManager.Instance.DustPuff(groundCheck.position);
            }
        }

        Vector3 scale = transform.localScale;
        if (moveInput > 0)       scale.x =  Mathf.Abs(scale.x);
        else if (moveInput < 0)  scale.x = -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    void FixedUpdate()
    {
        float speed = isRunning ? runSpeed : walkSpeed;
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        if (rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (collision.contacts[0].normal.y > 0.5f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 0.8f);
                collision.gameObject.SendMessage("OnStomp", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                if (playerState != null)
                {
                    if (playerState.hasStarPower)
                        collision.gameObject.SendMessage("OnStomp", SendMessageOptions.DontRequireReceiver);
                    else
                        playerState.TakeDamage();
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}