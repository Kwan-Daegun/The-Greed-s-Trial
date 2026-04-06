using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class QuestionBlock : MonoBehaviour
{
    public GameObject mushroomPrefab;
    public Transform spawnPoint;

    [Header("Bounce")]
    public float bounceHeight = 0.2f;
    public float bounceSpeed  = 6f;

    [Header("Used State")]
    public Sprite usedSprite;

    [Header("Hit Flash")]
    public Color hitFlashColor = new Color(1f, 1f, 0.5f, 1f);

    private bool used = false;
    private Vector3 originalPos;
    private SpriteRenderer sr;
    private Light2D blockLight;

    void Start()
    {
        originalPos = transform.position;
        sr = GetComponent<SpriteRenderer>();
        blockLight = GetComponentInChildren<Light2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (used) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                StartCoroutine(ActivateBlock());
                break;
            }
        }
    }

    IEnumerator ActivateBlock()
    {
        used = true;

        StartCoroutine(ScalePunch());
        if (sr != null) StartCoroutine(ColorFlash());
        if (blockLight != null) StartCoroutine(LightFlash());

        if (EffectsManager.Instance != null)
            EffectsManager.Instance.BlockHit(transform.position + Vector3.up * 0.5f);

        Vector3 upPos = originalPos + Vector3.up * bounceHeight;
        float t = 0;
        while (t < 1) { t += Time.deltaTime * bounceSpeed; transform.position = Vector3.Lerp(originalPos, upPos, Mathf.SmoothStep(0,1,t)); yield return null; }

        if (mushroomPrefab != null)
            Instantiate(mushroomPrefab, spawnPoint.position, Quaternion.identity);

        if (sr != null && usedSprite != null)
            sr.sprite = usedSprite;

        t = 0;
        while (t < 1) { t += Time.deltaTime * bounceSpeed * 1.5f; transform.position = Vector3.Lerp(upPos, originalPos, Mathf.SmoothStep(0,1,t)); yield return null; }
        transform.position = originalPos;
    }

    IEnumerator ScalePunch()
    {
        Vector3 orig = transform.localScale;
        Vector3 punch = new Vector3(orig.x * 1.25f, orig.y * 0.8f, orig.z);
        float t = 0f;
        while (t < 1f) { t += Time.deltaTime * 22f; transform.localScale = Vector3.Lerp(orig, punch, Mathf.SmoothStep(0,1,t)); yield return null; }
        t = 0f;
        while (t < 1f) { t += Time.deltaTime * 14f; transform.localScale = Vector3.Lerp(punch, orig, Mathf.SmoothStep(0,1,t)); yield return null; }
        transform.localScale = orig;
    }

    IEnumerator ColorFlash()
    {
        Color orig = sr.color;
        sr.color = hitFlashColor;
        yield return new WaitForSeconds(0.06f);
        sr.color = orig;
    }

    IEnumerator LightFlash()
    {
        float orig = blockLight.intensity;
        blockLight.intensity = 3.5f;
        float t = 0f;
        yield return new WaitForSeconds(0.05f);
        while (t < 1f) { t += Time.deltaTime * 7f; blockLight.intensity = Mathf.Lerp(3.5f, orig, t); yield return null; }
        blockLight.intensity = orig;
    }
}