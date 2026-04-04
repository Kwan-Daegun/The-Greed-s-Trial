using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAnimation : MonoBehaviour
{
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private PlayerMovement movement;

    public Sprite[] idleFrames;
    public Sprite[] runFrames;
    public Sprite[] jumpFrames;
    public Sprite[] fallFrames;

    public float frameRate = 10f;

    [HideInInspector] public bool forceRun = false;

    private Sprite[] currentFrames;
    private int frameIndex;
    private float timer;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        bool isGrounded = Physics2D.OverlapCircle(
            movement.groundCheck.position,
            movement.groundCheckRadius,
            movement.groundLayer
        );

        float xVel = rb.linearVelocity.x;
        float yVel = rb.linearVelocity.y;

        Sprite[] targetFrames;

        if (!isGrounded)
        {
            targetFrames = yVel > 0 ? jumpFrames : fallFrames;
        }
        else
        {
            targetFrames = (Mathf.Abs(xVel) > 0.1f || forceRun) ? runFrames : idleFrames;
        }

        if (currentFrames != targetFrames)
        {
            currentFrames = targetFrames;
            frameIndex = 0;
            timer = 0f;
        }

        Animate();
    }

    void Animate()
    {
        if (currentFrames == null || currentFrames.Length == 0) return;

        timer += Time.deltaTime;

        if (timer >= 1f / frameRate)
        {
            timer = 0f;
            frameIndex++;

            if (frameIndex >= currentFrames.Length)
                frameIndex = 0;

            sr.sprite = currentFrames[frameIndex];
        }
    }
}