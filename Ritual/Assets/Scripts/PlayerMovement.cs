
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;

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
        horizontal = 0f;

        //W, player faces forward, but shows back view
        if (Input.GetKey(KeyCode.W))
        {
            idleDirection = 2f;

            animator.SetBool("IsWalking", false);
            animator.SetFloat("IdleDirection", idleDirection);
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

        //if no keys are pressed, set idle direction and no movement
        else
        {
            animator.SetBool("IsWalking", false);
            animator.SetFloat("IdleDirection", idleDirection);
            Debug.Log("current idle direction: " + idleDirection.ToString());
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontal * moveSpeed, 0f);
    }
}
