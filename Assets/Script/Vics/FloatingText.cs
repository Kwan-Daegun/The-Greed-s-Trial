using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Attach to a prefab with a TextMeshPro component.
/// Spawned by effects — floats up and fades out.
/// </summary>
public class FloatingText : MonoBehaviour
{
    public float riseSpeed = 1.5f;
    public float lifetime = 0.8f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private TextMeshPro tmp;

    void Awake()
    {
        tmp = GetComponent<TextMeshPro>();
    }

    public void Init(string text, Color color)
    {
        if (tmp != null)
        {
            tmp.text = text;
            tmp.color = color;
        }
        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Color startColor = tmp != null ? tmp.color : Color.white;

        // Pop scale in
        transform.localScale = Vector3.zero;

        while (elapsed < lifetime)
        {
            float t = elapsed / lifetime;

            transform.position = startPos + Vector3.up * riseSpeed * t;

            // Scale: pop in then out
            float scaleT = t < 0.2f ? t / 0.2f : 1f - ((t - 0.2f) / 0.8f) * 0.3f;
            float sc = Mathf.Clamp(scaleT, 0f, 1.3f);
            transform.localScale = Vector3.one * sc;

            // Fade out in second half
            if (tmp != null)
            {
                float alpha = t < 0.5f ? 1f : 1f - ((t - 0.5f) / 0.5f);
                tmp.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}