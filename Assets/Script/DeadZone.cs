using UnityEngine;

public class DeadZone : MonoBehaviour
{
     void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerState ps = other.GetComponent<PlayerState>();
            if (ps != null)
                ps.Die();
        }
    }
}
