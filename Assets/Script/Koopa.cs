using UnityEngine;

public class Koopa : MonoBehaviour
{
    public float speed = 2f;
    private bool inShell = false;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.left * speed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.x != 0)
        {
            speed *= -1;
            rb.linearVelocity = Vector2.right * speed;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.contacts[0].normal.y < -0.5f)
            {
                if (!inShell)
                {
                    inShell = true;
                    rb.linearVelocity = Vector2.zero;
                }
                else
                {
                    rb.linearVelocity = new Vector2(8f, 0);
                }
            }
        }
    }
}