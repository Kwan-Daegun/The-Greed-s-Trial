using UnityEngine;

public class Goomba : MonoBehaviour
{
    public float speed = 2f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = new Vector2(-speed, 0);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.x != 0)
        {
            speed *= -1;
            rb.linearVelocity = new Vector2(speed, 0);
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.contacts[0].normal.y < -0.5f)
            {
                Destroy(gameObject); // stomped
            }
        }
    }
}