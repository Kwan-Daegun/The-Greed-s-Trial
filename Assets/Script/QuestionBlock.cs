using UnityEngine;

public class QuestionBlock : MonoBehaviour
{
     public GameObject itemPrefab;
    private bool used = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (used) return;

        if (collision.contacts[0].normal.y > 0.5f)
        {
            used = true;
            Instantiate(itemPrefab, transform.position + Vector3.up, Quaternion.identity);
        }
    }
}
