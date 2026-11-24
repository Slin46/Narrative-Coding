
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    public bool canMove = true;

    private Rigidbody2D rb;
    private Animator animator;

    private float horizontal;

    //0=Back, 1=Left, 2=Right
    private float idleDirection = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        animator.SetFloat("IdleDirection", idleDirection);
    }

    void Update()
    {
        if (Time.timeScale == 0f)
        {
            horizontal = 0f;
            animator.SetBool("IsWalking", false);
            return;
        }

        horizontal = 0f;

        //W, player faces forward, but shows back view
        if (Input.GetKey(KeyCode.W))
        {
            idleDirection = 2f;

            animator.SetBool("IsWalking", false);
            animator.SetFloat("IdleDirection", idleDirection);
        }

        //stop player movement when player presses W on triggerzone
        if (!canMove)
        {
            horizontal = 0f;
            animator.SetBool("IsWalking", false);
            rb.linearVelocity = Vector2.zero;
            return;
        }

        //A, walk left
        else if (Input.GetKey(KeyCode.A))
        {
            horizontal = -1f;
            idleDirection = 1f;

            animator.SetInteger("WalkDirection", -1);
            animator.SetFloat("IdleDirection", idleDirection);
            animator.SetBool("IsWalking", true);
        }

        //D, walk right
        else if (Input.GetKey(KeyCode.D))
        {
            horizontal = 1f;
            idleDirection = 0f;

            animator.SetInteger("WalkDirection", 1);
            animator.SetFloat("IdleDirection", idleDirection);
            animator.SetBool("IsWalking", true);
        }

        else if (Input.GetKey(KeyCode.S))
        {
            idleDirection = 3f;
            animator.SetBool("IsWalking", false);
            animator.SetFloat("IdleDirection", idleDirection);
        }

        //if no keys are pressed, set idle direction and no movement
        else
        {
            animator.SetBool("IsWalking", false);
            animator.SetFloat("IdleDirection", idleDirection);
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontal * moveSpeed, 0f);
    }
}
