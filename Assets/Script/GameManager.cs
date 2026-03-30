using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI timerText;
    public GameObject winPanel;
    public GameObject gameOverPanel;

    [Header("Timer")]
    public float timeLimit = 300f;
    private float currentTime;
    private bool isGameOver = false;

    private int coinCount = 0;
    private PlayerMovement player;

    void Start()
    {
        player = FindObjectOfType<PlayerMovement>();

        currentTime = timeLimit;
        Time.timeScale = 1f;

        if (winPanel != null) winPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (isGameOver) return;

        UpdateTimer();
        UpdateUI();
    }

    void UpdateTimer()
    {
        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            StartCoroutine(GameOver());
        }
    }

    void UpdateUI()
    {
        if (coinText != null)
            coinText.text = "Coins: " + coinCount;

        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    public void AddCoin()
    {
        coinCount++;
    }

    public IEnumerator GameOver()
    {
        isGameOver = true;
        yield return new WaitForSecondsRealtime(0.5f);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (player != null)
            player.enabled = false;

        Time.timeScale = 0f;
    }

    public void WinGame()
    {
        if (winPanel != null)
            winPanel.SetActive(true);

        if (player != null)
            player.enabled = false;

        Time.timeScale = 0f;
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextLevel(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}