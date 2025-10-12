using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float groundDist = 0.2f; // jarak raycast ke bawah
    public float fallMultiplier = 2f; // multiplier biar jatuh lebih natural

    [Header("References")]
    public LayerMask terrainLayer;
    public Rigidbody rb;
    public SpriteRenderer sr;
    public Animator animator;

    private float xInput;
    private float yInput;
    private bool isGrounded;

    void Start()
    {
        if (rb == null) rb = gameObject.GetComponent<Rigidbody>();
        if (sr == null) sr = gameObject.GetComponentInChildren<SpriteRenderer>();
        if (animator == null) animator = gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        CheckGround();
        HandleMovement();
        HandleAnimation();
        ApplyExtraGravity();
    }

    private void CheckGround()
    {
        // Raycast ke bawah untuk cek apakah grounded
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.5f; // sedikit di atas kaki
        if (Physics.Raycast(origin, Vector3.down, out hit, groundDist + 0.5f, terrainLayer))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void HandleMovement()
    {
        xInput = Input.GetAxis("Horizontal");
        yInput = Input.GetAxis("Vertical");

        Vector3 moveDir = new Vector3(xInput, 0, yInput);
        Vector3 velocity = moveDir * speed;
        velocity.y = rb.linearVelocity.y; // biar gravity tetap bekerja

        rb.linearVelocity = velocity;

        // Flip sprite sesuai arah
        if (xInput < 0) sr.flipX = true;
        else if (xInput > 0) sr.flipX = false;
    }

    private void ApplyExtraGravity()
    {
        if (!isGrounded && rb.linearVelocity.y < 0)
        {
            // tambahin gravity multiplier biar jatuh lebih cepat/natural
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
    }

    private void HandleAnimation()
    {
        // Set parameter animator
        bool isMoving = (xInput != 0 || yInput != 0);
        animator.SetBool("isRunning", isMoving);
    }
}
