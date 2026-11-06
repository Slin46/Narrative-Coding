
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    private Rigidbody2D rb;
    private Animator animator;

    private float horizontal;

    // 0 = Back, 1 = Left, 2 = Right
    private int idleDirection = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        animator.SetInteger("IdleDirection", idleDirection);
        animator.SetBool("IsWalking", false);
    }

    void Update()
    {
        horizontal = 0f;

        // W: Face back view
        if (Input.GetKey(KeyCode.W))
        {
            idleDirection = 0;

            animator.SetBool("IsWalking", false);
            animator.SetInteger("IdleDirection", idleDirection);
        }

        // A: Walk left
        else if (Input.GetKey(KeyCode.A))
        {
            horizontal = -1f;
            idleDirection = 1;

            animator.SetInteger("IdleDirection", idleDirection);
            animator.SetInteger("WalkDirection", -1);
            animator.SetBool("IsWalking", true);
        }

        // D: Walk right
        else if (Input.GetKey(KeyCode.D))
        {
            horizontal = 1f;
            idleDirection = 2;

            animator.SetInteger("IdleDirection", idleDirection);
            animator.SetInteger("WalkDirection", 1);
            animator.SetBool("IsWalking", true);
        }

        // No keys: Remember last facing direction
        else
        {
            animator.SetBool("IsWalking", false);
            animator.SetInteger("IdleDirection", idleDirection);
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontal * moveSpeed, 0f);
    }
}
