using System.Collections;
using UnityEngine;
using TMPro;
using Cinemachine;

public class SwitchDoorControl : MonoBehaviour
{
    public enum MoveDirection { Up, Down, Left, Right }
    [Header("Lever Animation")]
    public Animator leverAnimator;

    [Header("Door Settings")]
    public GameObject doorToOpen;
    public GameObject doorToClose;
    public float moveDistance = 3f;
    public float moveSpeed = 2f;

    [Header("Direction Settings")]
    public MoveDirection openDoorDirection = MoveDirection.Up;
    public MoveDirection closeDoorDirection = MoveDirection.Down;

    [Header("Player Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public float interactRange = 2f;
    public TextMeshProUGUI interactText;

    [Header("Cinemachine Settings")]
    public CinemachineVirtualCamera cinemachineCam;
    public float cameraFocusTime = 1.5f;

    private Transform player;
    private bool isMoving = false;
    private bool isOpen = false;
    private Vector3 openDoorStartPos;
    private Vector3 closeDoorStartPos;
    private Transform defaultCameraFollow;
    private Rigidbody2D playerRb;
    private PlayerMovement playerMovementScript;
    private Animator playerAnimator; //  Added animator reference

    private void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerRb = playerObj.GetComponent<Rigidbody2D>();
            playerMovementScript = playerObj.GetComponent<PlayerMovement>();
            playerAnimator = playerObj.GetComponent<Animator>(); //  Get animator
        }

        if (interactText != null)
            interactText.gameObject.SetActive(false);

        if (doorToOpen == null && transform.Find("DoorA") != null)
            doorToOpen = transform.Find("DoorA").gameObject;

        if (doorToClose == null && transform.Find("DoorB") != null)
            doorToClose = transform.Find("DoorB").gameObject;

        if (doorToOpen != null)
            openDoorStartPos = doorToOpen.transform.position;

        if (doorToClose != null)
            closeDoorStartPos = doorToClose.transform.position;

        if (cinemachineCam != null)
            defaultCameraFollow = cinemachineCam.Follow;
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(player.position, transform.position);

        if (distance <= interactRange && !isMoving)
        {
            if (interactText != null)
            {
                interactText.gameObject.SetActive(true);
                interactText.text = "Press [E]";
            }

            if (Input.GetKeyDown(interactKey))
                StartCoroutine(ToggleDoorsSequence());
        }
        else
        {
            if (interactText != null)
                interactText.gameObject.SetActive(false);
        }
    }

    private IEnumerator ToggleDoorsSequence()
    {
        isMoving = true;
        if (interactText != null)
            interactText.gameObject.SetActive(false);

        FreezePlayer(true);

        Vector3 openDir = GetDirectionVector(openDoorDirection);
        Vector3 closeDir = GetDirectionVector(closeDoorDirection);

        Vector3 openStart = doorToOpen != null ? doorToOpen.transform.position : Vector3.zero;
        Vector3 closeStart = doorToClose != null ? doorToClose.transform.position : Vector3.zero;

        Vector3 openTarget = isOpen ? openDoorStartPos : openStart + openDir * moveDistance;
        Vector3 closeTarget = isOpen ? closeDoorStartPos : closeStart + closeDir * moveDistance;
        if (leverAnimator != null)
            leverAnimator.SetBool("isOn", !isOpen);

        if (cinemachineCam != null && doorToOpen != null)
        {
            cinemachineCam.Follow = doorToOpen.transform;
            yield return new WaitForSeconds(cameraFocusTime);
        }

        if (doorToOpen != null)
            yield return StartCoroutine(MoveDoor(doorToOpen, openStart, openTarget));

        if (cinemachineCam != null && doorToClose != null)
        {
            cinemachineCam.Follow = doorToClose.transform;
            yield return new WaitForSeconds(cameraFocusTime);
        }

        if (doorToClose != null)
            yield return StartCoroutine(MoveDoor(doorToClose, closeStart, closeTarget));

        if (cinemachineCam != null)
        {
            cinemachineCam.Follow = defaultCameraFollow;
            yield return new WaitForSeconds(cameraFocusTime);
        }

        isOpen = !isOpen;
        isMoving = false;

        FreezePlayer(false); // Unfreeze player
    }

    // Same freeze logic as DoorAndKey
    private void FreezePlayer(bool freeze)
    {
        if (playerMovementScript != null)
            playerMovementScript.enabled = !freeze;

        if (playerRb != null)
        {
            playerRb.velocity = Vector2.zero;
            playerRb.isKinematic = freeze;
        }

        if (playerAnimator != null)
            playerAnimator.speed = freeze ? 0f : 1f;
    }

    private IEnumerator MoveDoor(GameObject door, Vector3 startPos, Vector3 targetPos)
    {
        if (door == null) yield break;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed / moveDistance;
            door.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        door.transform.position = targetPos;
    }

    private Vector3 GetDirectionVector(MoveDirection direction)
    {
        switch (direction)
        {
            case MoveDirection.Up: return Vector3.up;
            case MoveDirection.Down: return Vector3.down;
            case MoveDirection.Left: return Vector3.left;
            case MoveDirection.Right: return Vector3.right;
            default: return Vector3.zero;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
