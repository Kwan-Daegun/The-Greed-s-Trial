using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public int health;
    public int numOfHearts;
    public Sprite emptyHeart;
    public Sprite fullHeart;
    public Image[] Heart;

    Vector2 startPos;
    Vector2 respawnPoint;
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    [Header("Ground Check Settings")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.35f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private Animator animator;
    public GameManager gm;
    public GameObject splashEffect;
    public float distance = 1f;
    public LayerMask boxMask;
    GameObject box;

    [Header("Checkpoint Sound")]
    public AudioClip CheckPointSound;
    [Range(0f, 10f)] public float volume = 10f;

    [Header("Death Sound")]
    public AudioClip deathSound;
    [Range(0f, 10f)] public float deathVolume = 10f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        startPos = transform.position;
        respawnPoint = startPos;
    }

    void Update()
    {
        Physics2D.queriesStartInColliders = false;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        animator.SetBool("Isjumping", !isGrounded);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right * transform.localScale.x, distance, boxMask);

        if (hit.collider != null && hit.collider.CompareTag("Box") && Input.GetKeyDown(KeyCode.J) && isGrounded)
        {
            box = hit.collider.gameObject;
            FixedJoint2D joint = box.GetComponent<FixedJoint2D>();
            joint.enabled = true;
            joint.connectedBody = rb;
        }

        if (Input.GetKeyUp(KeyCode.J))
        {
            if (box != null)
            {
                FixedJoint2D joint = box.GetComponent<FixedJoint2D>();
                if (joint != null)
                {
                    joint.enabled = false;
                    joint.connectedBody = null;
                }
                box = null;
            }
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);

        for (int i = 0; i < Heart.Length; i++)
        {
            if (health > numOfHearts)
                health = numOfHearts;

            Heart[i].sprite = i < health ? fullHeart : emptyHeart;
            Heart[i].enabled = i < numOfHearts;
        }

        if (health <= 0)
            gm.StartCoroutine(gm.GameOver());
    }

    void FixedUpdate()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        animator.SetFloat("Xvelocity", Math.Abs(rb.velocity.x));
        animator.SetFloat("Yvelocity", rb.velocity.y);

        if (moveInput > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Trap"))
        {
            health--;
            Instantiate(splashEffect, transform.position, Quaternion.identity);
            Die();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("CheckPoint"))
        {
            if (CheckPointSound != null)
                AudioSource.PlayClipAtPoint(CheckPointSound, transform.position, volume);

            respawnPoint = other.transform.position;
            Debug.Log("Checkpoint reached!");
        }

        if (other.gameObject.CompareTag("Finish"))
            gm.WinGame();
    }

    void Die()
    {
        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position, deathVolume);

        StartCoroutine(RespawnAfterDelay(1f));
    }

    IEnumerator RespawnAfterDelay(float delay)
    {
        rb.velocity = Vector2.zero;
        rb.simulated = false;
        GetComponent<SpriteRenderer>().enabled = false;

        yield return new WaitForSeconds(delay);
        Respawn();
    }

    void Respawn()
    {
        transform.position = respawnPoint;
        rb.simulated = true;
        GetComponent<SpriteRenderer>().enabled = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + Vector2.right * transform.localScale.x * distance);
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
