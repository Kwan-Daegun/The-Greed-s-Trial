using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Attach this + a Light2D component to any torch/lantern GameObject.
/// Creates organic warm flickering light. No prefabs needed.
/// </summary>
[RequireComponent(typeof(Light2D))]
public class TorchLight : MonoBehaviour
{
    [Header("Flicker")]
    public float baseIntensity = 1.2f;
    public float flickerAmount = 0.25f;
    public float flickerSpeed  = 8f;

    [Header("Radius Pulse")]
    public float baseRadius = 4f;
    public float radiusPulse = 0.3f;
    public float pulseSpeed  = 3f;

    [Header("Color")]
    public Color warmColor = new Color(1f, 0.60f, 0.15f);
    public Color coolColor = new Color(1f, 0.75f, 0.30f);

    private Light2D light2D;
    private float noiseOffset;

    void Awake()
    {
        light2D = GetComponent<Light2D>();
        noiseOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        float t = Time.time;
        float n1 = Mathf.PerlinNoise(t * flickerSpeed, noiseOffset);
        float n2 = Mathf.PerlinNoise(noiseOffset, t * flickerSpeed * 0.7f);

        light2D.intensity = baseIntensity + (n1 - 0.5f) * 2f * flickerAmount;

        float pulse = Mathf.Sin(t * pulseSpeed + noiseOffset) * 0.5f + 0.5f;
        light2D.pointLightOuterRadius = baseRadius + pulse * radiusPulse;
        light2D.pointLightInnerRadius = Mathf.Max(0f, light2D.pointLightOuterRadius - 1.5f);

        light2D.color = Color.Lerp(warmColor, coolColor, n2);
    }
}