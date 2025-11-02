using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CeilingCrush : MonoBehaviour
{
    public float moveSpeed = 2f;       // How fast the ceiling moves
    public float minY = 1f;            // Lowest point before it stops or kills player
    public float maxY = 5f;            // Starting height (resets to this)
    public bool isActive = false;      // Whether ceiling is currently moving
    public bool repeat = false;        // Whether it goes up and down repeatedly

    private bool movingDown = true;

    void Update()
    {
        if (!isActive) return;

        // Move the ceiling
        if (movingDown)
            transform.position += Vector3.down * moveSpeed * Time.deltaTime;
        else
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // Stop or reverse when limits reached
        if (transform.position.y <= minY)
        {
            if (repeat) movingDown = false;
            else isActive = false;
        }

        if (transform.position.y >= maxY && repeat)
        {
            movingDown = true;
        }
    }

    // Optional: start crushing via trigger or event
    public void Activate()
    {
        isActive = true;
    }
}
