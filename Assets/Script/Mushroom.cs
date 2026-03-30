using UnityEngine;

public class Mushroom : MonoBehaviour
{
    public float speed = 2f;

    void Start()
    {
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.right * speed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.transform.localScale *= 1.5f;
            Destroy(gameObject);
        }
    }
}