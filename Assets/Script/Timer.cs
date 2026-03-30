using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public float time = 300f;
    public TextMeshProUGUI text;

    void Update()
    {
        time -= Time.deltaTime;
        text.text = Mathf.Ceil(time).ToString();

        if (time <= 0)
        {
            // Kill player
        }
    }
}