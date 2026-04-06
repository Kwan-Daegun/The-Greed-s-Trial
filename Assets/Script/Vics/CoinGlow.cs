using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Attach to coin GameObjects for a gentle bob + light pulse.
/// Does NOT lock startPos at Start — safe for coins placed or spawned at any time.
/// </summary>
public class CoinGlow : MonoBehaviour
{
    [Header("Bob")]
    public float bobHeight = 0.1f;
    public float bobSpeed  = 2.5f;

    [Header("Light Pulse (needs a Light2D child — fully optional)")]
    public float minIntensity = 0.5f;
    public float maxIntensity = 1.1f;
    public float pulseSpeed   = 3f;

    private Light2D light2D;
    private float timeOffset;
    private float baseY;
    private bool initialized = false;

    void Awake()
    {
        light2D = GetComponentInChildren<Light2D>();
    }

    // Use LateStart pattern — wait one frame so the coin is fully placed
    void OnEnable()
    {
        initialized = false;
    }

    void Update()
    {
        // Grab Y on the first real update frame, after any spawn positioning is done
        if (!initialized)
        {
            baseY       = transform.position.y;
            timeOffset  = Random.Range(0f, Mathf.PI * 2f);
            initialized = true;
        }

        // Bob only on Y — never touch X/Z so physics/movement still work
        Vector3 pos = transform.position;
        pos.y = baseY + Mathf.Sin(Time.time * bobSpeed + timeOffset) * bobHeight;
        transform.position = pos;

        // Light pulse
        if (light2D != null)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed + timeOffset) * 0.5f + 0.5f;
            light2D.intensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);
        }
    }
}