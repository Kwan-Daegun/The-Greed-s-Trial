using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════
    // INSPECTOR FIELDS
    // ═══════════════════════════════════════════════════════════════

    [Header("── HUD ──")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI timerText;

    [Header("── Panels ──")]
    public GameObject winPanel;
    public GameObject gameOverPanel;
    public GameObject pausePanel;

    [Header("── Overlay (full-screen Image, alpha 0) ──")]
    public Image flashOverlay;

    [Header("── Atmosphere ──")]
    public Light2D globalLight;
    public float deathLightIntensity = 0.01f;

    [Header("── Timer ──")]
    public float timeLimit        = 300f;
    public float lowTimeThreshold = 30f;

    [Header("── Scenes ──")]
    public string mainMenuSceneName = "MainMenu";

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE STATE
    // ═══════════════════════════════════════════════════════════════

    private float           currentTime;
    private bool            isGameOver       = false;
    private int             coinCount        = 0;
    private bool            isPaused         = false;
    private float           originalLightIntensity;
    private PlayerMovement  player;

    private Coroutine coinBounceCoroutine;
    private Coroutine timerPulseCoroutine;
    private bool      lowTimeTriggered = false;

    private RectTransform winPanelRT;
    private RectTransform gameOverPanelRT;
    private RectTransform pausePanelRT;

    // Particle pools for UI rain effects
    private List<RectTransform> particlePool = new List<RectTransform>();
    private Canvas uiCanvas;

    // ═══════════════════════════════════════════════════════════════
    // LIFECYCLE
    // ═══════════════════════════════════════════════════════════════

    void Start()
    {
        player      = FindObjectOfType<PlayerMovement>();
        currentTime = timeLimit;
        Time.timeScale = 1f;

        uiCanvas = FindObjectOfType<Canvas>();

        if (globalLight != null)
            originalLightIntensity = globalLight.intensity;

        CachePanel(winPanel,      ref winPanelRT);
        CachePanel(gameOverPanel, ref gameOverPanelRT);
        CachePanel(pausePanel,    ref pausePanelRT);

        if (flashOverlay != null)
        {
            flashOverlay.gameObject.SetActive(true);
            flashOverlay.color = Color.clear;
            flashOverlay.raycastTarget = false;
        }

        StartCoroutine(HUDSlideIn());
    }

    void CachePanel(GameObject panel, ref RectTransform rt)
    {
        if (panel == null) return;
        rt = panel.GetComponent<RectTransform>();
        panel.SetActive(false);
    }

    void Update()
    {
        if (isGameOver) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else          Pause();
        }

        if (isPaused) return;

        if (player == null) { StartCoroutine(GameOver()); return; }

        UpdateTimer();
        UpdateHUD();
    }

    // ═══════════════════════════════════════════════════════════════
    // TIMER
    // ═══════════════════════════════════════════════════════════════

    void UpdateTimer()
    {
        currentTime -= Time.deltaTime;
        if (currentTime <= 0f) { currentTime = 0f; StartCoroutine(GameOver()); }

        if (!lowTimeTriggered && currentTime <= lowTimeThreshold)
        {
            lowTimeTriggered = true;
            if (timerPulseCoroutine != null) StopCoroutine(timerPulseCoroutine);
            timerPulseCoroutine = StartCoroutine(LowTimerPulse());
        }
    }

    void UpdateHUD()
    {
        if (coinText  != null) coinText.text  = "✦ " + coinCount;
        if (timerText != null)
        {
            int m = Mathf.FloorToInt(currentTime / 60f);
            int s = Mathf.FloorToInt(currentTime % 60f);
            timerText.text  = $"{m:00}:{s:00}";
            timerText.color = currentTime > lowTimeThreshold
                ? Color.white
                : Color.Lerp(new Color(1f, 0.15f, 0.15f), Color.white, currentTime / lowTimeThreshold);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // COINS
    // ═══════════════════════════════════════════════════════════════

    public void AddCoin()
    {
        coinCount++;
        if (coinBounceCoroutine != null) StopCoroutine(coinBounceCoroutine);
        if (coinText != null) coinBounceCoroutine = StartCoroutine(CoinTextBounce());
    }

    // ═══════════════════════════════════════════════════════════════
    // PAUSE
    // ═══════════════════════════════════════════════════════════════

    public void Pause()
    {
        if (isGameOver) return;
        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            StartCoroutine(PanelPopIn(pausePanelRT));
            StartCoroutine(FrostOverlay(true));
        }
        if (player != null) player.enabled = false;
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        StartCoroutine(FrostOverlay(false));
        if (pausePanel != null)
            StartCoroutine(PanelPopOut(pausePanelRT, () => pausePanel.SetActive(false)));
        if (player != null) player.enabled = true;
    }

    // ═══════════════════════════════════════════════════════════════
    // GAME OVER  —  "The Shadow Descent"
    // ═══════════════════════════════════════════════════════════════

    public IEnumerator GameOver()
    {
        if (isGameOver) yield break;
        isGameOver = true;

        // 1. Slow-motion + red tint
        Time.timeScale = 0.3f;
        yield return StartCoroutine(FlashScreenUnscaled(Color.clear, new Color(0.6f, 0f, 0f, 0.35f), 0.6f));

        // 2. Freeze + full dark descent
        Time.timeScale = 0f;

        float elapsed = 0f;
        while (elapsed < 1.2f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / 1.2f;
            if (globalLight  != null) globalLight.intensity = Mathf.Lerp(originalLightIntensity, deathLightIntensity, t);
            if (flashOverlay != null) flashOverlay.color     = Color.Lerp(new Color(0.6f,0,0,0.35f), new Color(0,0,0,0.88f), t);
            yield return null;
        }

        // 3. Panel slams in
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            yield return StartCoroutine(PanelSlam(gameOverPanelRT));
            StartCoroutine(ScanlineShimmer(gameOverPanel));
            StartCoroutine(SpawnAshParticles(gameOverPanel));
        }

        if (player != null) player.enabled = false;
    }

    // ═══════════════════════════════════════════════════════════════
    // WIN  —  "The Golden Bloom"
    // ═══════════════════════════════════════════════════════════════

    public void WinGame()
    {
        StartCoroutine(WinSequence());
    }

    IEnumerator WinSequence()
    {
        isGameOver = true;
        if (player != null) player.enabled = false;

        // 1. Golden flash
        yield return StartCoroutine(FlashScreenUnscaled(Color.clear, new Color(1f, 0.9f, 0.3f, 0.8f), 0.2f));
        yield return StartCoroutine(FlashScreenUnscaled(new Color(1f, 0.9f, 0.3f, 0.8f), Color.clear, 0.35f));

        // 2. Brighten world briefly
        if (globalLight != null)
            StartCoroutine(LightBloom());

        Time.timeScale = 0f;

        // 3. Win panel blooms in
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            yield return StartCoroutine(PanelBloomIn(winPanelRT));
            StartCoroutine(SpawnGoldConfetti(winPanel));
            StartCoroutine(PanelGlowPulse(winPanelRT, new Color(1f, 0.85f, 0.2f)));
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // SCENE MANAGEMENT
    // ═══════════════════════════════════════════════════════════════

    public void Retry()
    {
        Time.timeScale = 1f;
        StartCoroutine(FadeAndLoad(SceneManager.GetActiveScene().name));
    }

    public void NextLevel(string sceneName)
    {
        Time.timeScale = 1f;
        StartCoroutine(FadeAndLoad(sceneName));
    }

    public void backToMenu(string sceneName)
    {
        Time.timeScale = 1f;
        StartCoroutine(FadeAndLoad(sceneName));
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        yield return StartCoroutine(FlashScreenUnscaled(Color.clear, Color.black, 0.4f));
        SceneManager.LoadScene(sceneName);
    }

    // ═══════════════════════════════════════════════════════════════
    // HUD ANIMATIONS
    // ═══════════════════════════════════════════════════════════════

    IEnumerator HUDSlideIn()
    {
        RectTransform coinRT  = coinText  != null ? (RectTransform)coinText.transform  : null;
        RectTransform timerRT = timerText != null ? (RectTransform)timerText.transform : null;

        Vector2 coinEnd  = coinRT  != null ? coinRT.anchoredPosition  : Vector2.zero;
        Vector2 timerEnd = timerRT != null ? timerRT.anchoredPosition : Vector2.zero;

        if (coinRT  != null) coinRT.anchoredPosition  = coinEnd  + Vector2.up * 120f;
        if (timerRT != null) timerRT.anchoredPosition = timerEnd + Vector2.up * 120f;

        // Small delay so first frame is clean
        yield return new WaitForSeconds(0.1f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            float e = EaseOutBack(Mathf.Clamp01(t));
            if (coinRT  != null) coinRT.anchoredPosition  = Vector2.LerpUnclamped(coinEnd  + Vector2.up * 120f, coinEnd,  e);
            if (timerRT != null) timerRT.anchoredPosition = Vector2.LerpUnclamped(timerEnd + Vector2.up * 120f, timerEnd, e);
            yield return null;
        }
        if (coinRT  != null) coinRT.anchoredPosition  = coinEnd;
        if (timerRT != null) timerRT.anchoredPosition = timerEnd;
    }

    IEnumerator CoinTextBounce()
    {
        if (coinText == null) yield break;
        RectTransform rt = coinText.rectTransform;
        Vector3 orig = Vector3.one;
        Vector3 big  = new Vector3(1.55f, 1.55f, 1f);

        // Flash gold
        coinText.color = new Color(1f, 0.95f, 0.3f);

        float t = 0f;
        while (t < 1f) { t += Time.unscaledDeltaTime * 16f; rt.localScale = Vector3.LerpUnclamped(orig, big, EaseOutBack(Mathf.Clamp01(t))); yield return null; }
        t = 0f;
        while (t < 1f) { t += Time.unscaledDeltaTime * 11f; rt.localScale = Vector3.Lerp(big, orig, Mathf.SmoothStep(0,1,t)); yield return null; }
        rt.localScale  = orig;
        coinText.color = Color.white;
    }

    IEnumerator LowTimerPulse()
    {
        if (timerText == null) yield break;
        RectTransform rt = timerText.rectTransform;
        while (!isGameOver && currentTime > 0f)
        {
            // Heartbeat: two quick pulses then rest
            yield return ScalePulse(rt, 1.25f, 14f);
            yield return new WaitForSeconds(0.12f);
            yield return ScalePulse(rt, 1.15f, 14f);
            yield return new WaitForSeconds(0.5f);
        }
        rt.localScale = Vector3.one;
    }

    IEnumerator ScalePulse(RectTransform rt, float targetScale, float speed)
    {
        Vector3 orig = Vector3.one;
        Vector3 big  = Vector3.one * targetScale;
        float t = 0f;
        while (t < 1f) { t += Time.deltaTime * speed; rt.localScale = Vector3.Lerp(orig, big, Mathf.SmoothStep(0,1,t)); yield return null; }
        t = 0f;
        while (t < 1f) { t += Time.deltaTime * speed; rt.localScale = Vector3.Lerp(big, orig, Mathf.SmoothStep(0,1,t)); yield return null; }
        rt.localScale = orig;
    }

    // ═══════════════════════════════════════════════════════════════
    // PANEL ANIMATIONS
    // ═══════════════════════════════════════════════════════════════

    // Standard pop — used for pause
    IEnumerator PanelPopIn(RectTransform rt)
    {
        if (rt == null) yield break;
        rt.localScale = Vector3.zero;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 6f;
            rt.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, EaseOutBack(Mathf.Clamp01(t)));
            yield return null;
        }
        rt.localScale = Vector3.one;
    }

    // Slams down from above — used for Game Over
    IEnumerator PanelSlam(RectTransform rt)
    {
        if (rt == null) yield break;
        Vector2 finalPos = rt.anchoredPosition;
        rt.anchoredPosition = finalPos + Vector2.up * 900f;
        rt.localScale = Vector3.one;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 5f;
            rt.anchoredPosition = Vector2.LerpUnclamped(finalPos + Vector2.up * 900f, finalPos, EaseOutBounce(Mathf.Clamp01(t)));
            yield return null;
        }
        rt.anchoredPosition = finalPos;

        // Brief squash on landing
        yield return StartCoroutine(SquashAndSettle(rt));
    }

    IEnumerator SquashAndSettle(RectTransform rt)
    {
        Vector3 squash  = new Vector3(1.15f, 0.88f, 1f);
        Vector3 stretch = new Vector3(0.92f, 1.08f, 1f);
        float t = 0f;
        while (t < 1f) { t += Time.unscaledDeltaTime * 20f; rt.localScale = Vector3.Lerp(Vector3.one, squash,  Mathf.SmoothStep(0,1,t)); yield return null; }
        t = 0f;
        while (t < 1f) { t += Time.unscaledDeltaTime * 14f; rt.localScale = Vector3.Lerp(squash,  stretch, Mathf.SmoothStep(0,1,t)); yield return null; }
        t = 0f;
        while (t < 1f) { t += Time.unscaledDeltaTime * 10f; rt.localScale = Vector3.Lerp(stretch, Vector3.one, Mathf.SmoothStep(0,1,t)); yield return null; }
        rt.localScale = Vector3.one;
    }

    // Blooms from center with overshoot — used for Win
    IEnumerator PanelBloomIn(RectTransform rt)
    {
        if (rt == null) yield break;
        rt.localScale = Vector3.zero;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 4f;
            rt.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, EaseOutElastic(Mathf.Clamp01(t)));
            yield return null;
        }
        rt.localScale = Vector3.one;
    }

    IEnumerator PanelPopOut(RectTransform rt, System.Action onDone = null)
    {
        if (rt == null) { onDone?.Invoke(); yield break; }
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 9f;
            rt.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, Mathf.SmoothStep(0,1,t));
            yield return null;
        }
        rt.localScale = Vector3.zero;
        onDone?.Invoke();
    }

    // Subtle glow pulse on panel background image
    IEnumerator PanelGlowPulse(RectTransform rt, Color glowColor)
    {
        if (rt == null) yield break;
        Image bg = rt.GetComponent<Image>();
        if (bg == null) bg = rt.GetComponentInChildren<Image>();
        if (bg == null) yield break;

        Color baseColor = bg.color;
        while (true)
        {
            float pulse = Mathf.Sin(Time.unscaledTime * 2f) * 0.5f + 0.5f;
            bg.color = Color.Lerp(baseColor, glowColor, pulse * 0.18f);
            yield return null;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // ATMOSPHERE EFFECTS
    // ═══════════════════════════════════════════════════════════════

    // Pause: faint blue-grey tint over the game world
    IEnumerator FrostOverlay(bool fadingIn)
    {
        if (flashOverlay == null) yield break;
        Color target = fadingIn ? new Color(0.3f, 0.4f, 0.55f, 0.35f) : Color.clear;
        Color start  = flashOverlay.color;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 7f;
            flashOverlay.color = Color.Lerp(start, target, t);
            yield return null;
        }
        flashOverlay.color = target;
    }

    // Win: briefly boost the global light
    IEnumerator LightBloom()
    {
        if (globalLight == null) yield break;
        float bloom = originalLightIntensity * 4f;
        float t = 0f;
        while (t < 1f) { t += Time.unscaledDeltaTime * 6f; globalLight.intensity = Mathf.Lerp(originalLightIntensity, bloom, t); yield return null; }
        t = 0f;
        while (t < 1f) { t += Time.unscaledDeltaTime * 3f; globalLight.intensity = Mathf.Lerp(bloom, originalLightIntensity, t); yield return null; }
        globalLight.intensity = originalLightIntensity;
    }

    // ═══════════════════════════════════════════════════════════════
    // UI PARTICLE SYSTEMS  (all built in code — zero prefabs)
    // ═══════════════════════════════════════════════════════════════

    // Gold confetti rains down on the Win panel
    IEnumerator SpawnGoldConfetti(GameObject panel)
    {
        if (uiCanvas == null) yield break;
        RectTransform canvasRT = uiCanvas.GetComponent<RectTransform>();
        float W = canvasRT.rect.width;
        float H = canvasRT.rect.height;

        for (int i = 0; i < 55; i++)
        {
            GameObject p = new GameObject("Confetti");
            p.transform.SetParent(uiCanvas.transform, false);

            Image img = p.AddComponent<Image>();
            // Alternate between squares and narrow rectangles
            bool isSquare = (i % 3 != 0);
            RectTransform rt = p.GetComponent<RectTransform>();
            rt.sizeDelta = isSquare ? new Vector2(10, 10) : new Vector2(5, 16);
            rt.anchoredPosition = new Vector2(Random.Range(-W * 0.5f, W * 0.5f), H * 0.5f + 20f);

            // Warm palette: golds, oranges, off-whites
            Color[] palette = {
                new Color(1f, 0.88f, 0.2f),
                new Color(1f, 0.65f, 0.1f),
                new Color(1f, 1f, 0.7f),
                new Color(1f, 0.4f, 0.15f),
                new Color(0.9f, 0.9f, 0.9f),
            };
            img.color = palette[Random.Range(0, palette.Length)];

            StartCoroutine(AnimateConfetti(rt, img, H));
            yield return new WaitForSecondsRealtime(0.04f);
        }
    }

    IEnumerator AnimateConfetti(RectTransform rt, Image img, float canvasHeight)
    {
        float fallSpeed   = Random.Range(280f, 520f);
        float drift       = Random.Range(-60f, 60f);
        float spinSpeed   = Random.Range(120f, 400f) * (Random.value > 0.5f ? 1 : -1);
        float wobble      = Random.Range(30f, 90f);
        float wobbleSpeed = Random.Range(2f, 5f);
        float startX      = rt.anchoredPosition.x;
        float elapsed     = 0f;
        float lifetime    = Random.Range(2.0f, 3.5f);

        while (elapsed < lifetime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / lifetime;

            rt.anchoredPosition = new Vector2(
                startX + drift * t + Mathf.Sin(elapsed * wobbleSpeed) * wobble,
                rt.anchoredPosition.y - fallSpeed * Time.unscaledDeltaTime
            );
            rt.localRotation = Quaternion.Euler(0, 0, rt.localEulerAngles.z + spinSpeed * Time.unscaledDeltaTime);

            // Fade out last 30%
            if (t > 0.7f)
            {
                Color c = img.color;
                c.a = Mathf.Lerp(1f, 0f, (t - 0.7f) / 0.3f);
                img.color = c;
            }
            yield return null;
        }
        Destroy(rt.gameObject);
    }

    // Dark ash / ember particles drift up on Game Over
    IEnumerator SpawnAshParticles(GameObject panel)
    {
        if (uiCanvas == null) yield break;
        RectTransform canvasRT = uiCanvas.GetComponent<RectTransform>();
        float W = canvasRT.rect.width;
        float H = canvasRT.rect.height;

        for (int i = 0; i < 40; i++)
        {
            GameObject p = new GameObject("Ash");
            p.transform.SetParent(uiCanvas.transform, false);

            Image img = p.AddComponent<Image>();
            RectTransform rt = p.GetComponent<RectTransform>();
            float size = Random.Range(3f, 9f);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = new Vector2(Random.Range(-W * 0.5f, W * 0.5f), -H * 0.5f - 10f);

            Color[] ashColors = {
                new Color(0.9f, 0.3f, 0.1f, 0.8f),
                new Color(0.6f, 0.6f, 0.6f, 0.6f),
                new Color(1f, 0.5f, 0.1f, 0.7f),
                new Color(0.3f, 0.3f, 0.3f, 0.5f),
            };
            img.color = ashColors[Random.Range(0, ashColors.Length)];

            StartCoroutine(AnimateAsh(rt, img, H));
            yield return new WaitForSecondsRealtime(0.06f);
        }
    }

    IEnumerator AnimateAsh(RectTransform rt, Image img, float canvasHeight)
    {
        float riseSpeed   = Random.Range(60f, 160f);
        float drift       = Random.Range(-40f, 40f);
        float wobble      = Random.Range(15f, 50f);
        float wobbleSpeed = Random.Range(1f, 3f);
        float startX      = rt.anchoredPosition.x;
        float elapsed     = 0f;
        float lifetime    = Random.Range(3f, 6f);

        while (elapsed < lifetime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / lifetime;

            rt.anchoredPosition = new Vector2(
                startX + drift * t + Mathf.Sin(elapsed * wobbleSpeed) * wobble,
                rt.anchoredPosition.y + riseSpeed * Time.unscaledDeltaTime
            );

            Color c = img.color;
            c.a = t < 0.2f ? t / 0.2f : Mathf.Lerp(1f, 0f, (t - 0.2f) / 0.8f);
            img.color = c;
            yield return null;
        }
        Destroy(rt.gameObject);
    }

    // Scanline shimmer: subtle horizontal line pulses across panel
    IEnumerator ScanlineShimmer(GameObject panel)
    {
        if (uiCanvas == null) yield break;
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        if (panelRT == null) yield break;

        for (int pass = 0; pass < 3; pass++)
        {
            GameObject line = new GameObject("Scanline");
            line.transform.SetParent(panel.transform, false);

            Image img = line.AddComponent<Image>();
            img.color = new Color(1f, 0.85f, 0.3f, 0f);

            RectTransform rt = line.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.sizeDelta = new Vector2(0f, 3f);

            float panelH  = panelRT.rect.height;
            float startY  = -panelH * 0.5f - 10f;
            float endY    =  panelH * 0.5f + 10f;
            float elapsed = 0f;
            float duration = 0.7f;

            yield return new WaitForSecondsRealtime(pass * 0.5f + 0.2f);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                rt.anchoredPosition = new Vector2(0f, Mathf.Lerp(startY, endY, t));
                img.color = new Color(1f, 0.85f, 0.3f, Mathf.Sin(t * Mathf.PI) * 0.6f);
                yield return null;
            }
            Destroy(line);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // SCREEN FLASH
    // ═══════════════════════════════════════════════════════════════

    IEnumerator FlashScreenUnscaled(Color from, Color to, float duration)
    {
        if (flashOverlay == null) yield break;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            flashOverlay.color = Color.Lerp(from, to, t);
            yield return null;
        }
        flashOverlay.color = to;
    }

    // ═══════════════════════════════════════════════════════════════
    // EASING LIBRARY
    // ═══════════════════════════════════════════════════════════════

    float EaseOutBack(float t)
    {
        float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    float EaseOutBounce(float t)
    {
        float n1 = 7.5625f, d1 = 2.75f;
        if      (t < 1f / d1)       return n1 * t * t;
        else if (t < 2f / d1)       return n1 * (t -= 1.5f   / d1) * t + 0.75f;
        else if (t < 2.5f / d1)     return n1 * (t -= 2.25f  / d1) * t + 0.9375f;
        else                        return n1 * (t -= 2.625f  / d1) * t + 0.984375f;
    }

    float EaseOutElastic(float t)
    {
        if (t <= 0f) return 0f;
        if (t >= 1f) return 1f;
        float c4 = (2f * Mathf.PI) / 3f;
        return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
    }
}