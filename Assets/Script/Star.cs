using UnityEngine;

public class Star : MonoBehaviour
{
    public float speed = 4f;
    public float bounceForce = 8f;

    [Header("Movement")]
    public Transform wallCheck;
    public float wallCheckDistance = 0.1f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private float direction = 1f;
    private bool collected = false;

    private Vector3 wallCheckStartPos;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        wallCheckStartPos = wallCheck.localPosition;
    }

    void Start()
    {
        rb.linearVelocity = new Vector2(direction * speed, bounceForce);
    }

    void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            wallCheck.position,
            Vector2.right * direction,
            wallCheckDistance,
            groundLayer
        );

        if (hit.collider != null)
        {
            direction *= -1f;
            Flip();
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collected) return;

        if (collision.CompareTag("Player"))
        {
            PlayerState ps = collision.GetComponent<PlayerState>();
            if (ps != null) ps.ActivateStar();

            collected = true;
            Destroy(gameObject);
        }
    }

    void Flip()
    {
        wallCheck.localPosition = new Vector3(
            wallCheckStartPos.x * direction,
            wallCheckStartPos.y,
            wallCheckStartPos.z
        );
    }
}