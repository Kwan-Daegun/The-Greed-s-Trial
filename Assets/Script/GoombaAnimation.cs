using UnityEngine;

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
        if (isDead)
        {
            SetAnimation(deadFrames, deadFrameRate);
        }
        else
        {
            SetAnimation(walkFrames, walkFrameRate);
        }

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

        if (timer >= interval)
        {
            timer = 0f;
            frameIndex++;

            if (frameIndex >= currentFrames.Length)
            {
                if (isDead)
                {
                    frameIndex = currentFrames.Length - 1;
                    if (!deathFinished)
                    {
                        deathFinished = true;
                        Destroy(gameObject, 0.1f);
                    }
                }
                else
                {
                    frameIndex = 0;
                }
            }

            sr.sprite = currentFrames[frameIndex];
        }
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
    }
}