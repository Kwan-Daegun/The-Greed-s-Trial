using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class KoopaAnimation : MonoBehaviour
{
    public Sprite[] walkFrames;
    public Sprite[] shellFrames;

    public float walkFrameRate = 8f;
    public float shellFrameRate = 12f;

    private SpriteRenderer sr;
    private Koopa koopa;

    private int frameIndex;
    private float timer;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        koopa = GetComponent<Koopa>();
    }

    void Update()
    {
        if (koopa == null) return;

        if (!koopa.inShell)
        {
            Animate(walkFrames, walkFrameRate);
        }
        else if (koopa.isMovingShell)
        {
            Animate(shellFrames, shellFrameRate);
        }
        else
        {
            if (shellFrames.Length > 0)
                sr.sprite = shellFrames[0];
        }
    }

    void Animate(Sprite[] frames, float rate)
    {
        if (frames == null || frames.Length == 0) return;

        timer += Time.deltaTime;

        if (timer >= 1f / rate)
        {
            timer = 0f;
            frameIndex++;

            if (frameIndex >= frames.Length)
                frameIndex = 0;

            sr.sprite = frames[frameIndex];
        }
    }
}