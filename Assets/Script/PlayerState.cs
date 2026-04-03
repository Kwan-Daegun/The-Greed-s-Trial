using UnityEngine;
using System.Collections;

public class PlayerState : MonoBehaviour
{
    [Header("Scale States")]
    public Vector3 smallScale = new Vector3(1f, 1f, 1f);
    public Vector3 bigScale = new Vector3(1.5f, 1.5f, 1f);

    [Header("Settings")]
    public float growDuration = 0.2f;
    public float invincibleTime = 1f;

    private bool isDead = false;
    private bool isInvincible = false;

    public bool isBig = false;
    private bool isGrowing = false;
    private bool isShrinking = false;

    public void Grow()
    {
        if (isBig || isGrowing) return;
        StartCoroutine(GrowRoutine());
    }

    IEnumerator GrowRoutine()
    {
        isGrowing = true;

        Vector3 start = transform.localScale;
        Vector3 target = new Vector3(
            Mathf.Sign(start.x) * bigScale.x,
            bigScale.y,
            bigScale.z
        );

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / growDuration;
            transform.localScale = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.localScale = target;
        isBig = true;
        isGrowing = false;
    }

    public void TakeDamage()
    {
        if (isDead || isInvincible) return;

        if (isBig)
            StartCoroutine(ShrinkRoutine());
        else
            Die();
    }

    IEnumerator ShrinkRoutine()
    {
        isInvincible = true;
        isShrinking = true;

        Vector3 start = transform.localScale;
        Vector3 target = new Vector3(
            Mathf.Sign(start.x) * smallScale.x,
            smallScale.y,
            smallScale.z
        );

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / growDuration;
            transform.localScale = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.localScale = target;
        isBig = false;
        isShrinking = false;

        yield return new WaitForSeconds(invincibleTime);
        isInvincible = false;
    }

    public void Die()
{
    if (isDead) return;
    isDead = true;

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
}