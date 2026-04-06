using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class Mushroom : MonoBehaviour
{
    public float speed = 2f;

    [Header("Movement")]
    public Transform wallCheck;
    public float wallCheckDistance = 0.1f;
    public LayerMask groundLayer;

    [Header("Glow (add a Light2D child — optional)")]
    public float minIntensity = 0.4f;
    public float maxIntensity = 0.9f;
    public float pulseSpeed   = 2.5f;

    private Rigidbody2D rb;
    private float direction = 1f;
    private bool collected  = false;
    private Vector3 wallCheckStartPos;
    private Light2D mushLight;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        wallCheckStartPos = wallCheck.localPosition;
        mushLight = GetComponentInChildren<Light2D>();
    }

    void Start()
    {
        rb.linearVelocity = Vector2.right * speed;

        if (EffectsManager.Instance != null)
            EffectsManager.Instance.MushroomPop(transform.position);

        StartCoroutine(SpawnPop());
    }

    IEnumerator SpawnPop()
    {
        Vector3 target = transform.localScale;
        transform.localScale = Vector3.zero;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 10f;
            float e = EaseOutBack(Mathf.Clamp01(t));
            transform.localScale = Vector3.LerpUnclamped(Vector3.zero, target, e);
            yield return null;
        }
        transform.localScale = target;
    }

    void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(wallCheck.position, Vector2.right * direction, wallCheckDistance, groundLayer);
        if (hit.collider != null) { direction *= -1f; Flip(); }

        if (mushLight != null)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;
            mushLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collected) return;
        if (!collision.CompareTag("Player")) return;

        PlayerState ps = collision.GetComponent<PlayerState>();
        if (ps != null && !ps.isBig) ps.Grow();

        if (EffectsManager.Instance != null)
            EffectsManager.Instance.MushroomPop(transform.position);

        collected = true;
        Destroy(gameObject);
    }

    void Flip()
    {
        wallCheck.localPosition = new Vector3(wallCheckStartPos.x * direction, wallCheckStartPos.y, wallCheckStartPos.z);
    }

    float EaseOutBack(float t)
    {
        float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}