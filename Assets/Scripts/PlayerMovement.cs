using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Aiming")]
    [SerializeField] private Transform aimPivot; // objeto hijo que apunta visualmente hacia el mouse (ej: el sprite del arma)

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Camera cam;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        // En un juego top-down no queremos gravedad ni que la física rote al personaje
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        ReadMovementInput();
        AimTowardMouse();
    }

    private void FixedUpdate()
    {
        // El movimiento físico va en FixedUpdate, no en Update, para que sea consistente sin importar el framerate
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    private void ReadMovementInput()
    {
        float x = Input.GetAxisRaw("Horizontal"); // A/D o flechas izquierda/derecha
        float y = Input.GetAxisRaw("Vertical");    // W/S o flechas arriba/abajo
        moveInput = new Vector2(x, y).normalized;   // normalizado para que la diagonal no sea más rápida
    }

    private void AimTowardMouse()
    {
        if (aimPivot == null) return; // si todavía no tienes un objeto de apuntado asignado, simplemente no hace nada

        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        Vector2 direction = mouseWorldPos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        aimPivot.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
