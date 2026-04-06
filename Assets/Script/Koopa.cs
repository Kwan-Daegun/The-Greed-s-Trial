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
    private float wallFlipCooldown = 0f;

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
        wallFlipCooldown -= Time.deltaTime;

        RaycastHit2D hit = Physics2D.Raycast(
            wallCheck.position,
            Vector2.right * direction,
            wallCheckDistance,
            groundLayer
        );

        if (hit.collider != null && wallFlipCooldown <= 0f)
        {
            direction *= -1;
            wallFlipCooldown = 0.2f;

            if (!inShell)
                rb.linearVelocity = new Vector2(direction * walkSpeed, rb.linearVelocity.y);
            else if (isMovingShell)
                rb.linearVelocity = new Vector2(direction * shellSpeed, rb.linearVelocity.y);

            if (isMovingShell && EffectsManager.Instance != null)
            {
                EffectsManager.Instance.DustPuff(transform.position);
                EffectsManager.Instance.Shake(0.07f, 0.1f);
            }

            Flip();
        }

        if (!inShell)
            rb.linearVelocity = new Vector2(direction * walkSpeed, rb.linearVelocity.y);
        else if (isMovingShell)
            rb.linearVelocity = new Vector2(direction * shellSpeed, rb.linearVelocity.y);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerState ps = collision.gameObject.GetComponent<PlayerState>();

            if (ps != null && ps.hasStarPower)
            {
                if (EffectsManager.Instance != null)
                    EffectsManager.Instance.EnemyDie(transform.position);
                Destroy(gameObject);
                return;
            }

            if (collision.relativeVelocity.y < -0.5f)
            {
                if (!inShell)           EnterShell();
                else if (!isMovingShell) KickShell(collision.transform);
                else                    StopShell();
            }
            else
            {
                if (ps != null) ps.TakeDamage();
            }
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (isMovingShell && collision.gameObject != gameObject)
            {
                if (EffectsManager.Instance != null)
                    EffectsManager.Instance.EnemyDie(collision.gameObject.transform.position);
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
        if (EffectsManager.Instance != null)
            EffectsManager.Instance.DustPuff(transform.position);
    }

    void KickShell(Transform player)
    {
        isMovingShell = true;
        float dir = player.position.x < transform.position.x ? 1 : -1;
        direction = (int)dir;
        rb.linearVelocity = new Vector2(direction * shellSpeed, rb.linearVelocity.y);
        Flip();

        if (EffectsManager.Instance != null)
            EffectsManager.Instance.Shake(0.1f, 0.15f);
    }

    void StopShell()
    {
        isMovingShell = false;
        rb.linearVelocity = Vector2.zero;
        if (EffectsManager.Instance != null)
            EffectsManager.Instance.DustPuff(transform.position);
    }

    void Flip()
    {
        if (sr != null) sr.flipX = direction < 0;
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
        Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.right * dir * wallCheckDistance);
        Gizmos.DrawWireSphere(wallCheck.position, 0.05f);
    }
}