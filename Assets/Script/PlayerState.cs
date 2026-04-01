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

    public void Grow()
    {
        if (Mathf.Abs(transform.localScale.x) > 1.1f) return;

        StartCoroutine(GrowRoutine());
    }

    IEnumerator GrowRoutine()
    {
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
    }

    public void TakeDamage()
    {
        if (isDead || isInvincible) return;

        if (Mathf.Abs(transform.localScale.x) > 1.1f)
        {
            StartCoroutine(ShrinkRoutine());
        }
        else
        {
            Die();
        }
    }

    IEnumerator ShrinkRoutine()
    {
        isInvincible = true;

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

        yield return new WaitForSeconds(invincibleTime);

        isInvincible = false;
    }

    void Die()
    {
        isDead = true;

        PlayerMovement pm = GetComponent<PlayerMovement>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (pm != null) pm.enabled = false;

        if (rb != null)
            rb.linearVelocity = new Vector2(0, 8f);

        Destroy(gameObject, 0.3f);
    }
}