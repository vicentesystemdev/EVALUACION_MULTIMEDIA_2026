using UnityEngine;
// Base lateral 2D: pon Rigidbody2D (Gravity Scale > 0), CapsuleCollider2D, suelo con Collider2D.
[RequireComponent(typeof(Rigidbody2D))]
public class CGPlayerSide2D : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 9f;
    public Transform groundCheck;
    public LayerMask groundMask;
    Rigidbody2D rb;
    float horizontal;
    void Awake() { rb = GetComponent<Rigidbody2D>(); rb.freezeRotation = true; }
    void Update()
    {
        // Ejemplo de prueba con Input antiguo. En proyectos con Input System nuevo,
        // configura Active Input Handling = Both o reemplaza con Input Actions.
        horizontal = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump") && groundCheck != null &&
            Physics2D.OverlapCircle(groundCheck.position, 0.11f, groundMask))
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        if (horizontal != 0) transform.localScale = new Vector3(Mathf.Sign(horizontal), 1, 1);
    }
    void FixedUpdate() { rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y); }
}
