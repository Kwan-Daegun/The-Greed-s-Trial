using UnityEngine;

public class Pipe : MonoBehaviour
{
    public Transform exitPoint;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (Input.GetAxis("Vertical") < -0.5f)
            {
                collision.transform.position = exitPoint.position;
            }
        }
    }
}