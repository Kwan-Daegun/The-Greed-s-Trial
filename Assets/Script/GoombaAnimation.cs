using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class GoombaAnimation : MonoBehaviour
{
    public Sprite[] walkFrames;
    public Sprite[] deadFrames;

    public float walkFrameRate = 8f;
    public float deadFrameRate = 10f;

    private SpriteRenderer sr;
    private Rigidbody2D rb;

    private Sprite[] currentFrames;
    private int frameIndex;
    private float timer;

    private bool isDead = false;
    private bool deathFinished = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        SetAnimation(isDead ? deadFrames : walkFrames, isDead ? deadFrameRate : walkFrameRate);
        Animate();
    }

    void SetAnimation(Sprite[] frames, float rate)
    {
        if (currentFrames == frames) return;
        currentFrames = frames;
        frameIndex = 0;
        timer = 0f;
    }

    void Animate()
    {
        if (currentFrames == null || currentFrames.Length == 0) return;
        timer += Time.deltaTime;
        float interval = 1f / (isDead ? deadFrameRate : walkFrameRate);
        if (timer < interval) return;

        timer = 0f;
        frameIndex++;
        if (frameIndex >= currentFrames.Length)
        {
            if (isDead)
            {
                frameIndex = currentFrames.Length - 1;
                if (!deathFinished) { deathFinished = true; Destroy(gameObject, 0.8f); }
            }
            else frameIndex = 0;
        }
        sr.sprite = currentFrames[frameIndex];
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        if (EffectsManager.Instance != null)
            EffectsManager.Instance.EnemyDie(transform.position);

        StartCoroutine(SquashDeath());
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
    }

    IEnumerator SquashDeath()
    {
        Vector3 orig = transform.localScale;
        Vector3 origPos = transform.position;
        Vector3 squash = new Vector3(orig.x * 1.7f, orig.y * 0.5f, orig.z);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 0.5f;
            float progress = Mathf.SmoothStep(0f, 1f, t);
            transform.localScale = Vector3.Lerp(orig, squash, progress);
            
            // Adjust Y position to keep Goomba grounded as it squashes
            float scaleProgress = Mathf.Lerp(orig.y, squash.y, progress);
            float heightLost = orig.y - scaleProgress;
            transform.position = origPos + Vector3.down * (heightLost / 0.5f);
            
            yield return null;
        }
        transform.localScale = squash;
        transform.position = origPos + Vector3.down * ((orig.y - squash.y) / 2f);
    }
}