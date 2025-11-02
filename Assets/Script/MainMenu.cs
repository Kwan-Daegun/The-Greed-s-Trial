using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public AudioClip bgMusic;
    [Range(0f, 1f)] public float volume = 1f;
    private AudioSource audioSource;

    [Header("Level Buttons")]
    public Button level2Button;
    public Button level3Button;

    void Start()
    {

        if (bgMusic != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = bgMusic;
            audioSource.volume = volume;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.Play();
        }

        
        int levelReached = PlayerPrefs.GetInt("levelReached", 1);

        if (level2Button != null)
            level2Button.interactable = levelReached >= 2;

        if (level3Button != null)
            level3Button.interactable = levelReached >= 3;
    }

    public void Play()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level 1");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Tutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void levelOne()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level 1");
    }

    public void levelTwo()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level 2");
    }

    public void levelThree()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level 3");
    }
}
