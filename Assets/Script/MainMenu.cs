using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required for Hover effects

public class MainMenu : MonoBehaviour
{
    [Header("── Audio & Atmosphere ──")]
    public AudioClip bgMusic;
    public AudioClip hoverSound;
    public AudioClip clickSound;
    [Range(0f, 1f)] public float volume = 1f;
    private AudioSource audioSource;
    private AudioSource sfxSource;

    [Header("── Level Buttons ──")]
    public Button playButton;
    public Button level2Button;
    public Button level3Button;
    public Button quitButton;

    [Header("── Visuals ──")]
    public Image fadeOverlay; // Assign a full-screen black image

    // ── Internal State ─────────────────────────────────────────────
    private bool isTransitioning = false;

    void Start()
    {
        // 1. Setup Audio Engine
        SetupAudio();

        // 2. Setup Persistence & Progression
        int levelReached = PlayerPrefs.GetInt("levelReached", 1);
        if (level2Button != null) level2Button.interactable = levelReached >= 2;
        if (level3Button != null) level3Button.interactable = levelReached >= 3;

        // 3. Setup Button "Juice" (Hover/Scale effects)
        SetupButtonEffects(playButton);
        SetupButtonEffects(level2Button);
        SetupButtonEffects(level3Button);
        SetupButtonEffects(quitButton);

        // 4. Start the scene with a Fade-In from black
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            StartCoroutine(FadeOverlay(1f, 0f, 1.5f));
        }
    }

    private void SetupAudio()
    {
        // Background Music with Pitch Control (for a slightly eerie vibe)
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = bgMusic;
        audioSource.volume = 0f; // Start silent for fade-in
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.Play();
        StartCoroutine(FadeAudio(0f, volume, 2f));

        // SFX Source for clicks/hovers
        sfxSource = gameObject.AddComponent<AudioSource>();
    }

    // ── Public API ────────────────────────────────────────────────

    public void Play() => StartTransition("Level 1");
    public void levelOne() => StartTransition("Level 1");
    public void levelTwo() => StartTransition("Level 2");
    public void levelThree() => StartTransition("Level 3");
    public void Tutorial() => StartTransition("Tutorial");

    public void Quit()
    {
        if (isTransitioning) return;
        PlaySFX(clickSound, 0.8f);
        StartCoroutine(FadeAudio(audioSource.volume, 0f, 0.5f));
        StartCoroutine(FadeOverlay(0f, 1f, 0.6f, () => Application.Quit()));
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ── The "10/10" Juice Logic ───────────────────────────────────

    private void StartTransition(string sceneName)
    {
        if (isTransitioning) return;
        isTransitioning = true;
        
        PlaySFX(clickSound, 1.1f);
        StartCoroutine(FadeAudio(audioSource.volume, 0f, 0.8f));
        StartCoroutine(FadeOverlay(0f, 1f, 0.8f, () => {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }));
    }

    private void SetupButtonEffects(Button btn)
    {
        if (btn == null) return;

        // Add EventTrigger to handle Hover (PointerEnter) and Unhover (PointerExit)
        EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>() ?? btn.gameObject.AddComponent<EventTrigger>();

        // Hover Enter
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => { OnButtonHover(btn); });
        trigger.triggers.Add(entry);

        // Hover Exit
        EventTrigger.Entry exit = new EventTrigger.Entry();
        exit.eventID = EventTriggerType.PointerExit;
        exit.callback.AddListener((data) => { OnButtonUnhover(btn); });
        trigger.triggers.Add(exit);

        // Click SFX
        btn.onClick.AddListener(() => { PlaySFX(clickSound, 1f); });
    }

    private void OnButtonHover(Button btn)
    {
        if (!btn.interactable || isTransitioning) return;
        PlaySFX(hoverSound, Random.Range(0.95f, 1.05f)); // Dynamic pitch shifting
        StartCoroutine(ScaleButton(btn.transform, 1.15f, 0.2f));
    }

    private void OnButtonUnhover(Button btn)
    {
        if (isTransitioning) return;
        StartCoroutine(ScaleButton(btn.transform, 1.0f, 0.2f));
    }

    // ── Coroutine Animations ──────────────────────────────────────

    IEnumerator ScaleButton(Transform target, float scale, float duration)
    {
        Vector3 startScale = target.localScale;
        Vector3 endScale = new Vector3(scale, scale, 1f);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            target.localScale = Vector3.LerpUnclamped(startScale, endScale, EaseOutBack(elapsed / duration));
            yield return null;
        }
        target.localScale = endScale;
    }

    IEnumerator FadeOverlay(float start, float end, float duration, System.Action onComplete = null)
    {
        if (fadeOverlay == null) { onComplete?.Invoke(); yield break; }
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            Color c = fadeOverlay.color;
            c.a = Mathf.Lerp(start, end, elapsed / duration);
            fadeOverlay.color = c;
            yield return null;
        }
        onComplete?.Invoke();
    }

    IEnumerator FadeAudio(float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        audioSource.volume = end;
    }

    private void PlaySFX(AudioClip clip, float pitch)
    {
        if (clip == null) return;
        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(clip);
    }

    // ── Easing ────────────────────────────────────────────────────
    float EaseOutBack(float t)
    {
        float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}