using UnityEngine;
using System.Collections;

public class QuestionBlock : MonoBehaviour
{
    public GameObject mushroomPrefab;
    public Transform spawnPoint;

    [Header("Bounce")]
    public float bounceHeight = 0.2f;
    public float bounceSpeed = 6f;

    [Header("Used State")]
    public Sprite usedSprite;

    private bool used = false;
    private Vector3 originalPos;
    private SpriteRenderer sr;

    void Start()
    {
        originalPos = transform.position;
        sr = GetComponent<SpriteRenderer>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (used) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    StartCoroutine(ActivateBlock());
                    break;
                }
            }
        }
    }

    IEnumerator ActivateBlock()
    {
        used = true;

        Vector3 upPos = originalPos + Vector3.up * bounceHeight;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * bounceSpeed;
            transform.position = Vector3.Lerp(originalPos, upPos, t);
            yield return null;
        }

        Instantiate(mushroomPrefab, spawnPoint.position, Quaternion.identity);

        if (sr != null && usedSprite != null)
            sr.sprite = usedSprite;

        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * (bounceSpeed * 1.5f);
            transform.position = Vector3.Lerp(upPos, originalPos, t);
            yield return null;
        }

        transform.position = originalPos;
    }
}