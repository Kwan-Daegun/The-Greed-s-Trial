using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Flagpole : MonoBehaviour
{
    public Transform poleTop;
    public Transform poleBottom;
    public Transform flagObject;
    public Transform house;
    public float slideSpeed = 4f;
    public float walkOffSpeed = 3f;

    private bool isTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isTriggered) return;
        if (collision.CompareTag("Player"))
        {
            isTriggered = true;
            StartCoroutine(FlagpoleSequence(collision.gameObject));
        }
    }

    IEnumerator FlagpoleSequence(GameObject player)
    {
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        PlayerState ps = player.GetComponent<PlayerState>();
        PlayerAnimation pa = player.GetComponent<PlayerAnimation>();
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (pm != null) pm.enabled = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (flagObject != null)
            flagObject.SetParent(null);

        float poleX = transform.position.x + 0.3f;
        player.transform.position = new Vector3(poleX, player.transform.position.y, player.transform.position.z);

        Vector3 scale = player.transform.localScale;
        scale.x = Mathf.Abs(scale.x);
        player.transform.localScale = scale;

        float bottomY = (poleBottom != null) ? poleBottom.position.y : (transform.position.y - 4f);

        while (player.transform.position.y > bottomY + 0.05f)
        {
            player.transform.position = Vector3.MoveTowards(
                player.transform.position,
                new Vector3(poleX, bottomY, player.transform.position.z),
                slideSpeed * Time.deltaTime
            );

            if (flagObject != null)
            {
                flagObject.position = Vector3.MoveTowards(
                    flagObject.position,
                    new Vector3(flagObject.position.x, bottomY + 0.5f, flagObject.position.z),
                    slideSpeed * Time.deltaTime
                );
            }

            yield return null;
        }

        player.transform.position = new Vector3(poleX, bottomY, player.transform.position.z);

        yield return new WaitForSeconds(0.1f);

        if (pa != null) pa.forceRun = true;

        float targetX = (house != null) ? house.position.x : player.transform.position.x + 5f;

        while (player.transform.position.x < targetX)
        {
            player.transform.position = Vector3.MoveTowards(
                player.transform.position,
                new Vector3(targetX, player.transform.position.y, player.transform.position.z),
                walkOffSpeed * Time.deltaTime
            );
            yield return null;
        }

        if (pa != null) pa.forceRun = false;

        yield return new WaitForSeconds(0.3f);

        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
            gm.WinGame();
        else
            SceneManager.LoadScene("WinScene");
    }
}