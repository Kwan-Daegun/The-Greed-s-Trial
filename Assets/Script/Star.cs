using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Star : MonoBehaviour
{
    public float speed       = 4f;
    public float bounceForce = 8f;

    [Header("Movement")]
    public Transform wallCheck;
    public float wallCheckDistance = 0.1f;
    public LayerMask groundLayer;

    [Header("Glow (add a Light2D child — optional)")]
    public float minLightIntensity = 0.8f;
    public float maxLightIntensity = 2.0f;
    public float pulseSpeed        = 4f;

    private Rigidbody2D rb;
    private float direction = 1f;
    private bool collected  = false;
    private Vector3 wallCheckStartPos;
    private SpriteRenderer sr;
    private Light2D starLight;

    private readonly Color[] rainbow = { Color.red, new Color(1f,0.5f,0f), Color.yellow, Color.green, Color.cyan, Color.blue, Color.magenta };
    private int colorIdx = 0;
    private float colorTimer = 0f;
    private const float COLOR_INTERVAL = 0.1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        wallCheckStartPos = wallCheck.localPosition;
        sr = GetComponent<SpriteRenderer>();
        starLight = GetComponentInChildren<Light2D>();
    }

    void Start()
    {
        rb.linearVelocity = new Vector2(direction * speed, bounceForce);
    }

    void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(wallCheck.position, Vector2.right * direction, wallCheckDistance, groundLayer);
        if (hit.collider != null) { direction *= -1f; Flip(); }

        // Glow pulse
        if (starLight != null)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;
            starLight.intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, pulse);
        }

        // Rainbow cycle
        colorTimer += Time.deltaTime;
        if (colorTimer >= COLOR_INTERVAL)
        {
            colorTimer = 0f;
            Color c = rainbow[colorIdx % rainbow.Length];
            colorIdx++;
            if (sr != null) sr.color = c;
            if (starLight != null) starLight.color = c;
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
            if (EffectsManager.Instance != null)
                EffectsManager.Instance.DustPuff(transform.position);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collected) return;
        if (!collision.CompareTag("Player")) return;

        PlayerState ps = collision.GetComponent<PlayerState>();
        if (ps != null) ps.ActivateStar();

        if (EffectsManager.Instance != null)
            EffectsManager.Instance.StarCollect(transform.position);

        collected = true;
        Destroy(gameObject);
    }

    void Flip()
    {
        wallCheck.localPosition = new Vector3(wallCheckStartPos.x * direction, wallCheckStartPos.y, wallCheckStartPos.z);
    }
}