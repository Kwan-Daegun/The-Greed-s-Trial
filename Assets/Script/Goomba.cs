using UnityEngine;

public class Goomba : MonoBehaviour
{
    public Transform wallCheck;
    public float wallCheckDistance = 0.1f;
    public LayerMask groundLayer;
    public float speed = 2f;

    private Rigidbody2D rb;
    private float currentDirection = -1f;
    private Vector3 wallCheckStartPos;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        wallCheckStartPos = wallCheck.localPosition;
    }

    void Start()
    {
        rb.linearVelocity = new Vector2(currentDirection * speed, rb.linearVelocity.y);
        Flip();
    }

    void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            wallCheck.position,
            Vector2.right * currentDirection,
            wallCheckDistance,
            groundLayer
        );

        if (hit.collider != null)
        {
            currentDirection *= -1f;
            Flip();
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(currentDirection * speed, rb.linearVelocity.y);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.relativeVelocity.y < -0.5f)
            {
                GetComponent<GoombaAnimation>().Die();
            }
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            currentDirection *= -1f;
            Flip();
        }
    }

    void Flip()
    {
        currentDirection = Mathf.Sign(currentDirection);

        wallCheck.localPosition = new Vector3(
            wallCheckStartPos.x * currentDirection,
            wallCheckStartPos.y,
            wallCheckStartPos.z
        );

        GetComponent<SpriteRenderer>().flipX = currentDirection > 0;
    }
    void OnDrawGizmosSelected()
    {
        if (wallCheck == null) return;

        Gizmos.color = Color.red;

        float direction = Application.isPlaying ? currentDirection : -1f;

        Gizmos.DrawLine(
            wallCheck.position,
            wallCheck.position + Vector3.right * direction * wallCheckDistance
        );

        Gizmos.DrawWireSphere(wallCheck.position, 0.05f);
    }
}