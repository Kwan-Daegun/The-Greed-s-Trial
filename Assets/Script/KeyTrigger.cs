using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyTrigger : MonoBehaviour
{
    private DoorAndKey parentDoor;

    private void Start()
    {
        parentDoor = GetComponentInParent<DoorAndKey>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            parentDoor.OnKeyTriggered();
        }
    }
}
