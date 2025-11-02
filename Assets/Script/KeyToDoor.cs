using UnityEngine;
using System.Collections;
using Cinemachine;

public class DoorAndKey : MonoBehaviour
{
    public enum MoveDirection { Up, Down, Left, Right }

    [Header("Door Settings")]
    public MoveDirection moveDirection = MoveDirection.Up;
    public float moveDistance = 5f;
    public float moveSpeed = 2f;

    [Header("Cinemachine Settings")]
    public float doorFocusTime = 2f;
    public CinemachineVirtualCamera vcamPlayer;
    public CinemachineVirtualCamera vcamDoor;

    private GameObject door;
    private GameObject key;
    private bool isMoving = false;

    private PlayerMovement playerMovement;
    private Rigidbody2D playerRb;
    private Animator playerAnimator;

    private void Awake()
    {
        door = transform.Find("Door")?.gameObject;
        key = transform.Find("Key")?.gameObject;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
            playerRb = player.GetComponent<Rigidbody2D>();
            playerAnimator = player.GetComponent<Animator>();
        }

        if (vcamPlayer == null)
            vcamPlayer = GameObject.FindWithTag("PlayerCamera")?.GetComponent<CinemachineVirtualCamera>();
        if (vcamDoor == null)
            vcamDoor = GameObject.FindWithTag("DoorCamera")?.GetComponent<CinemachineVirtualCamera>();
    }

    public void OnKeyTriggered()
    {
        if (!isMoving)
        {
            StartCoroutine(FocusOnDoorCoroutine());
            if (key != null) Destroy(key);
        }
    }

    private IEnumerator FocusOnDoorCoroutine()
    {
        FreezePlayer(true);

        if (vcamDoor != null && vcamPlayer != null)
        {
            vcamDoor.Priority = 20;
            vcamPlayer.Priority = 10;
        }

        StartCoroutine(SlideDoor());
        yield return new WaitForSeconds(doorFocusTime);

        if (vcamDoor != null && vcamPlayer != null)
        {
            vcamDoor.Priority = 10;
            vcamPlayer.Priority = 20;
        }

        yield return new WaitForSeconds(2f);

        FreezePlayer(false);
    }

    private void FreezePlayer(bool freeze)
    {
        if (playerMovement != null)
            playerMovement.enabled = !freeze;

        if (playerRb != null)
        {
            playerRb.velocity = Vector2.zero;
            playerRb.isKinematic = freeze;
        }

        if (playerAnimator != null)
            playerAnimator.speed = freeze ? 0f : 1f;
    }

    private IEnumerator SlideDoor()
    {
        if (door == null) yield break;
        isMoving = true;

        Vector3 startPos = door.transform.position;
        Vector3 direction = Vector3.zero;

        switch (moveDirection)
        {
            case MoveDirection.Up: direction = Vector3.up; break;
            case MoveDirection.Down: direction = Vector3.down; break;
            case MoveDirection.Left: direction = Vector3.left; break;
            case MoveDirection.Right: direction = Vector3.right; break;
        }

        Vector3 targetPos = startPos + direction * moveDistance;

        while (Vector3.Distance(door.transform.position, targetPos) > 0.01f)
        {
            door.transform.position = Vector3.MoveTowards(
                door.transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        door.transform.position = targetPos;
        isMoving = false;
    }
}
