using System.Collections;
using UnityEngine;
using TMPro;
using Cinemachine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject pauseButton;

    [Header("Win Condition")]
    public GameObject finishFlag;
    private int totalCoins;
    public int coinCount;
    public TextMeshProUGUI coinText;

    [Header("Win & Game Over Panels")]
    public GameObject winPanel;
    public GameObject gameOverPanel;
    private PlayerMovement pm;

    [Header("Cameras")]
    public CinemachineVirtualCamera playerCam;
    public CinemachineVirtualCamera flagCam;

    [Header("Timer Settings")]
    public float timeLimit = 300f;
    public TextMeshProUGUI timerText;
    private float currentTime;
    private bool isGameOver = false;
    public string sceneName;
    private bool isSwitchingCam = false;

    [Header("Background Music")]
    public AudioSource bgmSource;
    public AudioClip bgmClip;

    void Start()
    {
        pm = FindObjectOfType<PlayerMovement>();
        totalCoins = GameObject.FindGameObjectsWithTag("Coin").Length;

        if (finishFlag != null)
            finishFlag.SetActive(false);

        if (playerCam == null)
            playerCam = GameObject.Find("PlayerCam")?.GetComponent<CinemachineVirtualCamera>();
        if (flagCam == null)
            flagCam = GameObject.Find("FlagCam")?.GetComponent<CinemachineVirtualCamera>();

        if (playerCam != null && flagCam != null)
        {
            playerCam.Priority = 11;
            flagCam.Priority = 10;
            playerCam.enabled = true;
            flagCam.enabled = false;
        }

        if (winPanel != null)
            winPanel.SetActive(false);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Time.timeScale = 1f;
        currentTime = timeLimit;
        isGameOver = false;

        PlayBGM();
    }

    void PlayBGM()
    {
        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
            bgmSource.Stop();
    }

    void Update()
    {
        if (coinText != null)
            coinText.text = "Coins: " + coinCount;

        if (!isGameOver)
            UpdateTimer();
    }

    void UpdateTimer()
    {
        currentTime -= Time.deltaTime;

        if (currentTime < 0)
        {
            currentTime = 0;
            StartCoroutine(GameOver());
        }

        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = $"Timer: {minutes:00}:{seconds:00}";
        }
    }

    public IEnumerator GameOver()
    {
        isGameOver = true;
        yield return new WaitForSecondsRealtime(0.5f);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        if (pauseButton != null)
            pauseButton.SetActive(false);
        if (pm != null)
            pm.enabled = false;

        Time.timeScale = 0f;
    }

    public void AddCoin()
    {
        coinCount++;
        if (coinText != null)
            coinText.text = "Coins: " + coinCount;

        if (coinCount >= totalCoins)
        {
            if (finishFlag != null)
                finishFlag.SetActive(true);

            if (!isSwitchingCam)
                StartCoroutine(LookAtFlag());
        }
    }

    IEnumerator LookAtFlag()
    {
        isSwitchingCam = true;
        yield return new WaitForSecondsRealtime(0.1f);

        if (flagCam != null && playerCam != null)
        {
            flagCam.enabled = true;
            playerCam.enabled = false;
        }

        yield return new WaitForSecondsRealtime(2f);

        if (flagCam != null && playerCam != null)
        {
            flagCam.enabled = false;
            playerCam.enabled = true;
        }

        isSwitchingCam = false;
    }

    public void WinGame()
    {
       if (winPanel != null)
        winPanel.SetActive(true);
    if (pauseButton != null)
        pauseButton.SetActive(false);
    if (pm != null)
        pm.enabled = false;

    
    int currentLevel = SceneManager.GetActiveScene().buildIndex;
    int levelReached = PlayerPrefs.GetInt("levelReached", 1);
    if (currentLevel >= levelReached)
    {
        PlayerPrefs.SetInt("levelReached", currentLevel + 1);
        PlayerPrefs.Save();
    }


        Time.timeScale = 0f;
    }

    public void Pause() => Time.timeScale = 0f;
    public void Resume() => Time.timeScale = 1f;

    public void Retry()
    {
        StopAllCoroutines();
        StopBGM();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMenu()
    {
        StopAllCoroutines();
        StopBGM();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }

    public void nextLevel()
    {
        StopBGM();
        SceneManager.LoadScene(sceneName);
    }
}
