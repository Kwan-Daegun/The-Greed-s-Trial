using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Put this on your scene's Global Light 2D.
/// Keeps the stage dark but adds a subtle living flicker.
/// Recommended base intensity: 0.06 – 0.12
/// </summary>
[RequireComponent(typeof(Light2D))]
public class AmbientLightFlicker : MonoBehaviour
{
    [Tooltip("Base darkness level — keep low (0.06–0.12) for cave feel")]
    public float baseIntensity = 0.08f;
    public float flickerRange  = 0.012f;
    public float flickerSpeed  = 1.2f;

    private Light2D globalLight;
    private float noiseOffset;

    void Awake()
    {
        globalLight   = GetComponent<Light2D>();
        noiseOffset   = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (globalLight == null) return;
        float n = Mathf.PerlinNoise(Time.time * flickerSpeed, noiseOffset);
        globalLight.intensity = baseIntensity + (n - 0.5f) * 2f * flickerRange;
    }
}