using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class PlayerState : MonoBehaviour
{
    [Header("Scale States")]
    public Vector3 smallScale = new Vector3(1f, 1f, 1f);
    public Vector3 bigScale   = new Vector3(1.5f, 1.5f, 1f);

    [Header("Settings")]
    public float growDuration    = 0.2f;
    public float invincibleTime  = 1f;

    [Header("Star")]
    public float starDuration = 10f;
    public float flashSpeed   = 0.08f;

    [Header("Player Light (optional — add a Light2D child)")]
    public Light2D playerLight;
    public float normalLightIntensity = 0.25f;
    public float starLightIntensity   = 2.2f;

    private bool isDead      = false;
    private bool isInvincible = false;
    public  bool isBig        = false;
    private bool isGrowing    = false;

    public bool hasStarPower = false;
    private SpriteRenderer sr;
    private Coroutine starCoroutine;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (playerLight == null)
            playerLight = GetComponentInChildren<Light2D>();
    }

    void Start()
    {
        if (playerLight != null)
        {
            playerLight.intensity = normalLightIntensity;
            playerLight.color = Color.white;
        }
    }

    // ── Grow ──────────────────────────────────────────────────────

    public void Grow()
    {
        if (isBig || isGrowing) return;
        StartCoroutine(GrowRoutine());
    }

    IEnumerator GrowRoutine()
    {
        isGrowing = true;
        StartCoroutine(FlashColor(Color.white, 0.07f));

        Vector3 start = transform.localScale;
        Vector3 target = new Vector3(Mathf.Sign(start.x) * bigScale.x, bigScale.y, bigScale.z);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / growDuration;
            transform.localScale = Vector3.LerpUnclamped(start, target, EaseOutBack(Mathf.Clamp01(t)));
            yield return null;
        }
        transform.localScale = target;
        isBig = true;
        isGrowing = false;
    }

    // ── Damage ────────────────────────────────────────────────────

    public void TakeDamage()
    {
        if (isDead || isInvincible || hasStarPower) return;
        if (isBig) StartCoroutine(ShrinkRoutine());
        else       Die();
    }

    IEnumerator ShrinkRoutine()
    {
        isInvincible = true;

        StartCoroutine(FlashColor(new Color(1f, 0.25f, 0.25f), 0.1f));
        if (EffectsManager.Instance != null)
            EffectsManager.Instance.Shake(0.1f, 0.2f);

        Vector3 start  = transform.localScale;
        Vector3 target = new Vector3(Mathf.Sign(start.x) * smallScale.x, smallScale.y, smallScale.z);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / growDuration;
            transform.localScale = Vector3.Lerp(start, target, Mathf.Clamp01(t));
            yield return null;
        }
        transform.localScale = target;
        isBig = false;

        StartCoroutine(InvincibilityBlink());
        yield return new WaitForSeconds(invincibleTime);
        isInvincible = false;
        if (sr != null) sr.color = Color.white;
    }

    IEnumerator InvincibilityBlink()
    {
        float elapsed = 0f;
        while (elapsed < invincibleTime)
        {
            if (sr != null)
                sr.color = sr.color.a > 0.5f ? new Color(1,1,1,0.15f) : Color.white;
            yield return new WaitForSeconds(0.07f);
            elapsed += 0.07f;
        }
        if (sr != null) sr.color = Color.white;
    }

    IEnumerator FlashColor(Color flash, float duration)
    {
        if (sr == null) yield break;
        Color orig = sr.color;
        sr.color = flash;
        yield return new WaitForSeconds(duration);
        sr.color = orig;
    }

    // ── Star ──────────────────────────────────────────────────────

    public void ActivateStar()
    {
        if (starCoroutine != null) StopCoroutine(starCoroutine);
        starCoroutine = StartCoroutine(StarRoutine());
    }

    IEnumerator StarRoutine()
    {
        hasStarPower = true;
        isInvincible = true;

        if (EffectsManager.Instance != null)
            EffectsManager.Instance.StarCollect(transform.position);

        if (playerLight != null) playerLight.intensity = starLightIntensity;

        Color[] rainbow = { Color.red, new Color(1f,0.5f,0f), Color.yellow, Color.green, Color.cyan, Color.blue, Color.magenta };
        float elapsed = 0f;
        int idx = 0;

        while (elapsed < starDuration)
        {
            Color c = rainbow[idx % rainbow.Length];
            if (sr != null) sr.color = c;
            if (playerLight != null) playerLight.color = c;
            idx++;
            yield return new WaitForSeconds(flashSpeed);
            elapsed += flashSpeed;
        }

        if (sr != null) sr.color = Color.white;
        if (playerLight != null) { playerLight.intensity = normalLightIntensity; playerLight.color = Color.white; }
        hasStarPower = false;
        isInvincible = false;
        starCoroutine = null;
    }

    // ── Die ───────────────────────────────────────────────────────

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        if (EffectsManager.Instance != null)
            EffectsManager.Instance.PlayerDie(transform.position);

        PlayerMovement pm = GetComponent<PlayerMovement>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (pm != null) pm.enabled = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(0, 8f);
            rb.gravityScale = 1f;
        }
        Destroy(gameObject, 0.8f);
    }

    float EaseOutBack(float t)
    {
        float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}