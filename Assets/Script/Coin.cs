using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Coin Sound")]
    public AudioClip coinSound;
    [Range(0f, 1f)] public float volume = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (coinSound != null)
                AudioSource.PlayClipAtPoint(coinSound, transform.position, volume);

            if (EffectsManager.Instance != null)
                EffectsManager.Instance.CoinCollect(transform.position);

            FindObjectOfType<GameManager>().AddCoin();
            Destroy(gameObject);
        }
    }
}