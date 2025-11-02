using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class godWealth : MonoBehaviour
{
   public float amplitude = 0.5f; // height of movement
    public float frequency = 1f;   // speed of movement
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float y = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = startPos + new Vector3(0, y, 0);
    }
}
