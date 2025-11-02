using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BladeSpin : MonoBehaviour
{
    public float rotationSpeed;

    void FixedUpdate()
    {
        this.transform.Rotate(0, 0, rotationSpeed);

    }
    
}
