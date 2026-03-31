using UnityEngine;

public class Koopa : MonoBehaviour
{
    public float walkSpeed = 2f;
    public float shellSpeed = 8f;

    public Transform wallCheck;
    public float wallCheckDistance = 0.1f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private int direction = -1;

    public bool inShell = false;
    public bool isMovingShell = false;

    private Vector3 wallCheckStartPos;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        wallCheckStartPos = wallCheck.localPosition;
    }

    void Start()
    {
        rb.linearVelocity = new Vector2(direction * walkSpeed, rb.linearVelocity.y);
        Flip();
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
            direction *= -1;

            if (!inShell)
                rb.linearVelocity = new Vector2(direction * walkSpeed, rb.linearVelocity.y);
            else if (isMovingShell)
                rb.linearVelocity = new Vector2(direction * shellSpeed, rb.linearVelocity.y);

            Flip();
        }

        if (!inShell)
        {
            rb.linearVelocity = new Vector2(direction * walkSpeed, rb.linearVelocity.y);
        }
        else if (isMovingShell)
        {
            rb.linearVelocity = new Vector2(direction * shellSpeed, rb.linearVelocity.y);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            float playerY = collision.transform.position.y;
            float koopaY = transform.position.y;

            if (playerY > koopaY + 0.2f)
            {
                if (!inShell)
                    EnterShell();
                else if (!isMovingShell)
                    KickShell(collision.transform);
                else
                    StopShell();
            }

        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (isMovingShell && collision.gameObject != gameObject)
            {
                Destroy(collision.gameObject);
            }
            else
            {
                direction *= -1;
                Flip();

                if (!inShell)
                    rb.linearVelocity = new Vector2(direction * walkSpeed, rb.linearVelocity.y);
                else if (isMovingShell)
                    rb.linearVelocity = new Vector2(direction * shellSpeed, rb.linearVelocity.y);
            }
        }
    }

    void EnterShell()
    {
        inShell = true;
        isMovingShell = false;
        rb.linearVelocity = Vector2.zero;
    }

    void KickShell(Transform player)
    {
        isMovingShell = true;

        float dir = player.position.x < transform.position.x ? 1 : -1;
        direction = (int)dir;

        rb.linearVelocity = new Vector2(direction * shellSpeed, rb.linearVelocity.y);
        Flip();
    }

    void StopShell()
    {
        isMovingShell = false;
        rb.linearVelocity = Vector2.zero;
    }

    void Flip()
    {
        if (sr != null)
            sr.flipX = direction > 0;

        wallCheck.localPosition = new Vector3(
            wallCheckStartPos.x * direction,
            wallCheckStartPos.y,
            wallCheckStartPos.z
        );
    }

    void OnDrawGizmosSelected()
    {
        if (wallCheck == null) return;

        Gizmos.color = Color.cyan;

        float dir = Application.isPlaying ? direction : -1f;

        Vector3 start = wallCheck.position;
        Vector3 end = start + Vector3.right * dir * wallCheckDistance;

        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(start, 0.05f);
    }
}