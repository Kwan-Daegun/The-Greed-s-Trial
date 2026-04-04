using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Pipe : MonoBehaviour
{
    public Transform exitPoint;

    [Header("Timing")]
    public float slideDuration = 0.25f;
    public float fadeSpeed = 6f;

    [Header("Fade UI")]
    public Image fadeImage;

    private bool isTransitioning = false;

    void Awake()
    {
        if (exitPoint == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("ExitPipe");
            if (obj != null)
                exitPoint = obj.transform;
        }

        if (fadeImage == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("Fade");
            if (obj != null)
                fadeImage = obj.GetComponent<Image>();
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (isTransitioning) return;

        if (collision.CompareTag("Player"))
        {
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                StartCoroutine(EnterPipe(collision.transform));
            }
        }
    }

    IEnumerator EnterPipe(Transform player)
    {
        isTransitioning = true;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        PlayerMovement pm = player.GetComponent<PlayerMovement>();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (pm != null)
            pm.enabled = false;

        Vector3 startPos = player.position;
        Vector3 targetPos = startPos + Vector3.down * 1.5f;

        float t = 0f;
        while (t < slideDuration)
        {
            t += Time.deltaTime;
            float p = t / slideDuration;
            player.position = Vector3.Lerp(startPos, targetPos, p);
            yield return null;
        }

        yield return Fade(0f, 1f);

        player.position = exitPoint.position;

        startPos = player.position;
        targetPos = startPos + Vector3.up * 1.5f;

        t = 0f;
        while (t < slideDuration)
        {
            t += Time.deltaTime;
            float p = t / slideDuration;
            player.position = Vector3.Lerp(startPos, targetPos, p);
            yield return null;
        }

        yield return Fade(1f, 0f);

        if (pm != null)
            pm.enabled = true;

        isTransitioning = false;
    }

    IEnumerator Fade(float from, float to)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;
            float a = Mathf.Lerp(from, to, t);
            fadeImage.color = new Color(0, 0, 0, a);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, to);
    }
}