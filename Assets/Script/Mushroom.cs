using UnityEngine;

public class Mushroom : MonoBehaviour
{
    public float speed = 2f;

    [Header("Player Scale States")]
    public Vector3 smallScale = new Vector3(1f, 1f, 1f);
    public Vector3 bigScale = new Vector3(1.5f, 1.5f, 1f);

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
        rb.linearVelocity = Vector2.right * speed;
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

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collected) return;

        if (collision.CompareTag("Player"))
        {
            Transform player = collision.transform;

            if (Vector3.Distance(player.localScale, smallScale) < 0.01f)
            {
                PlayerState ps = collision.GetComponent<PlayerState>();
                if (ps != null)
                {
                    ps.Grow();
                }
            }

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