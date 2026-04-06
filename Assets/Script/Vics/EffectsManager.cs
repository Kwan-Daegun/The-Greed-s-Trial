using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EffectsManager : MonoBehaviour
{
    public static EffectsManager Instance;

    [Header("Camera (auto-finds Main Camera if empty)")]
    public Transform cameraTransform;
    private Vector3 cameraOriginalPos;
    private Coroutine shakeCoroutine;

    [Header("Post Processing (assign your Global Volume)")]
    public Volume postProcessVolume;
    private ChromaticAberration _chromatic;
    private Vignette _vignette;

    [Header("Hit Stop")]
    public bool useHitStop = true;

    // ── Lifecycle ─────────────────────────────────────────────────

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGet(out _chromatic);
            postProcessVolume.profile.TryGet(out _vignette);
        }
    }

    void Start()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
        if (cameraTransform != null)
            cameraOriginalPos = cameraTransform.localPosition;
    }

    // ── Public API ────────────────────────────────────────────────

    public void CoinCollect(Vector3 pos)
    {
        SpawnCoinBurst(pos);
    }

    public void DustPuff(Vector3 pos)
    {
        SpawnDust(pos);
    }

    public void EnemyDie(Vector3 pos)
    {
        SpawnEnemyPop(pos);
        Shake(0.15f, 0.20f);
        if (useHitStop) StartCoroutine(HitStopRoutine(0.05f));
        TriggerChromaticGlitch(0.4f, 0.5f);
    }

    public void BlockHit(Vector3 pos)
    {
        SpawnBlockStars(pos);
        Shake(0.08f, 0.12f);
        if (useHitStop) StartCoroutine(HitStopRoutine(0.03f));
    }

    public void MushroomPop(Vector3 pos)
    {
        SpawnMushroomSparkle(pos);
    }

    public void StarCollect(Vector3 pos)
    {
        SpawnRainbowBurst(pos);
        Shake(0.25f, 0.40f);
        if (useHitStop) StartCoroutine(HitStopRoutine(0.10f));
        TriggerChromaticGlitch(0.9f, 0.8f);
        TriggerVignetteFlash(new Color(0.8f, 0.7f, 0f), 0.6f);
    }

    public void PlayerJump(Vector3 pos)
    {
        SpawnDust(pos);
    }

    public void PlayerLand(Vector3 pos)
    {
        SpawnDust(pos);
        Shake(0.06f, 0.10f);
    }

    public void PlayerDie(Vector3 pos)
    {
        SpawnEnemyPop(pos);
        Shake(0.60f, 0.80f);
        if (useHitStop) StartCoroutine(HitStopRoutine(0.25f));
        TriggerChromaticGlitch(1.0f, 1.5f);
        TriggerVignetteFlash(new Color(0.5f, 0f, 0f), 1.2f);
    }

    // ── HitStop ───────────────────────────────────────────────────

    IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    // ── Screen Shake (unscaled — works during HitStop) ────────────

    public void Shake(float magnitude, float duration)
    {
        if (cameraTransform == null) return;
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(ShakeRoutine(magnitude, duration));
    }

    IEnumerator ShakeRoutine(float magnitude, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float mag = Mathf.Lerp(magnitude, 0f, elapsed / duration);
            cameraTransform.localPosition = cameraOriginalPos + (Vector3)Random.insideUnitCircle * mag;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        cameraTransform.localPosition = cameraOriginalPos;
    }

    // ── Chromatic Aberration Glitch ───────────────────────────────

    public void TriggerChromaticGlitch(float intensity, float fadeTime)
    {
        if (_chromatic == null) return;
        StartCoroutine(ChromaticRoutine(intensity, fadeTime));
    }

    IEnumerator ChromaticRoutine(float intensity, float fadeTime)
    {
        _chromatic.intensity.Override(intensity);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeTime;
            _chromatic.intensity.Override(Mathf.Lerp(intensity, 0f, t));
            yield return null;
        }
        _chromatic.intensity.Override(0f);
    }

    // ── Vignette Flash ────────────────────────────────────────────

    public void TriggerVignetteFlash(Color color, float fadeTime)
    {
        if (_vignette == null) return;
        StartCoroutine(VignetteRoutine(color, fadeTime));
    }

    IEnumerator VignetteRoutine(Color color, float fadeTime)
    {
        _vignette.color.Override(color);
        _vignette.intensity.Override(0.55f);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeTime;
            _vignette.intensity.Override(Mathf.Lerp(0.55f, 0f, t));
            yield return null;
        }
        _vignette.intensity.Override(0f);
    }

    // ── Material ──────────────────────────────────────────────────

    static Material _sharedMat;
    Material GetMat()
    {
        if (_sharedMat != null) return _sharedMat;
        // Sprite-Lit so particles are affected by the scene's 2D lighting
        Shader sh = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default")
                 ?? Shader.Find("Sprites/Default");
        _sharedMat = new Material(sh);
        return _sharedMat;
    }

    // ── Particle Factory ──────────────────────────────────────────

    ParticleSystem MakePS(string name, Vector3 pos, bool stretch = true, int sortOrder = 10)
    {
        GameObject go = new GameObject(name);
        go.transform.position = pos;
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var r = go.GetComponent<ParticleSystemRenderer>();
        r.material = GetMat();
        r.sortingLayerName = "Default";
        r.sortingOrder = sortOrder;

        if (stretch)
        {
            // Velocity-stretch makes fast particles feel snappy and physical
            r.renderMode    = ParticleSystemRenderMode.Stretch;
            r.velocityScale = 0.12f;
            r.lengthScale   = 0.18f;
        }
        else
        {
            r.renderMode = ParticleSystemRenderMode.Billboard;
        }

        return ps;
    }

    void FadeOverLifetime(ParticleSystem ps, Color startColor, Color endColor)
    {
        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(startColor, 0f), new GradientColorKey(endColor, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = new ParticleSystem.MinMaxGradient(g);
    }

    // ── Particles ─────────────────────────────────────────────────

    void SpawnCoinBurst(Vector3 pos)
    {
        ParticleSystem ps = MakePS("FX_Coin", pos);
        var main = ps.main;
        main.duration        = 0.3f;
        main.loop            = false;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.25f, 0.5f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(5f, 10f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.07f, 0.16f);
        main.startColor      = new ParticleSystem.MinMaxGradient(new Color(1f, 1f, 0.5f), new Color(1f, 0.75f, 0f));
        main.gravityModifier = 1.2f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ps.emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 20) });
        var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Circle; shape.radius = 0.05f;
        FadeOverLifetime(ps, new Color(1f, 1f, 0.5f), new Color(1f, 0.5f, 0f));
        ps.Play(); Destroy(ps.gameObject, 1.5f);
    }

    void SpawnDust(Vector3 pos)
    {
        ParticleSystem ps = MakePS("FX_Dust", pos, stretch: false);
        var main = ps.main;
        main.duration        = 0.3f;
        main.loop            = false;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.3f, 0.5f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(1f, 3f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
        main.startColor      = new ParticleSystem.MinMaxGradient(new Color(1,1,1,0.5f), new Color(0.7f,0.7f,0.7f,0.2f));
        main.gravityModifier = -0.05f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ps.emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 10) });
        var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = new Vector3(0.8f, 0.1f, 0.1f);
        FadeOverLifetime(ps, Color.white, new Color(0.5f, 0.5f, 0.5f, 0f));
        ps.Play(); Destroy(ps.gameObject, 1f);
    }

    void SpawnEnemyPop(Vector3 pos)
    {
        ParticleSystem ps = MakePS("FX_EnemyPop", pos);
        var main = ps.main;
        main.duration        = 0.3f;
        main.loop            = false;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.4f, 0.7f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(7f, 13f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.1f, 0.24f);
        main.startColor      = new ParticleSystem.MinMaxGradient(new Color(1f, 0.35f, 0.05f), new Color(0.9f, 0.8f, 0.1f));
        main.gravityModifier = 1.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ps.emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 25) });
        var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Circle; shape.radius = 0.12f;
        FadeOverLifetime(ps, new Color(1f, 0.4f, 0.05f), new Color(0.2f, 0.05f, 0f));

        // Noise jitter for explosive feel
        var noise = ps.noise;
        noise.enabled   = true;
        noise.strength  = 0.5f;
        noise.frequency = 0.8f;

        ps.Play(); Destroy(ps.gameObject, 1.5f);
    }

    void SpawnBlockStars(Vector3 pos)
    {
        ParticleSystem ps = MakePS("FX_BlockStars", pos);
        ps.gameObject.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        var main = ps.main;
        main.duration        = 0.3f;
        main.loop            = false;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(4f, 9f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
        main.startColor      = new ParticleSystem.MinMaxGradient(new Color(1f, 0.95f, 0.3f), new Color(1f, 0.55f, 0f));
        main.gravityModifier = 1.2f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ps.emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 14) });
        var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Cone; shape.angle = 35f; shape.radius = 0.1f;

        var noise = ps.noise; noise.enabled = true; noise.strength = 0.4f;
        FadeOverLifetime(ps, new Color(1f, 0.9f, 0.25f), new Color(1f, 0.4f, 0f));
        ps.Play(); Destroy(ps.gameObject, 1.5f);
    }

    void SpawnMushroomSparkle(Vector3 pos)
    {
        ParticleSystem ps = MakePS("FX_Mushroom", pos, stretch: false);
        var main = ps.main;
        main.duration        = 0.5f;
        main.loop            = false;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.45f, 0.85f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(1.5f, 4f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.06f, 0.16f);
        main.startColor      = new ParticleSystem.MinMaxGradient(new Color(0.5f, 1f, 0.5f), new Color(0.3f, 0.85f, 1f));
        main.gravityModifier = -0.2f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ps.emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 18) });
        var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Circle; shape.radius = 0.2f;
        FadeOverLifetime(ps, new Color(0.5f, 1f, 0.6f), Color.white);
        ps.Play(); Destroy(ps.gameObject, 1.5f);
    }

    void SpawnRainbowBurst(Vector3 pos)
    {
        Color[] cols = { Color.red, new Color(1f,0.5f,0f), Color.yellow, Color.green, Color.cyan, Color.blue, Color.magenta };
        for (int i = 0; i < cols.Length; i++)
        {
            ParticleSystem ps = MakePS("FX_Rainbow", pos);
            float angle = i * 360f / cols.Length;
            ps.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

            var main = ps.main;
            main.duration        = 0.3f;
            main.loop            = false;
            main.startLifetime   = new ParticleSystem.MinMaxCurve(0.4f, 0.75f);
            main.startSpeed      = new ParticleSystem.MinMaxCurve(9f, 15f);
            main.startSize       = new ParticleSystem.MinMaxCurve(0.07f, 0.17f);
            main.startColor      = cols[i];
            main.gravityModifier = 0.15f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ps.emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 6) });
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Cone; shape.angle = 10f; shape.radius = 0.05f;
            FadeOverLifetime(ps, cols[i], Color.white);
            ps.Play(); Destroy(ps.gameObject, 1.5f);
        }
    }
}