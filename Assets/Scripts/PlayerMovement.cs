using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Aiming")]
    [SerializeField] private Transform aimPivot; // objeto hijo que apunta visualmente hacia el mouse (ej: el sprite del arma)

    public SpriteRenderer WeaponSpriteRenderer { get; private set; }

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

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = new Color(0.35f, 0.85f, 1f);
        }

        BuildGlow();

        // Si no se asignó un pivote de apuntado en el Inspector, se crea uno solo
        // (una barra que atraviesa al jugador) para que siempre haya indicador visual de aim.
        if (aimPivot == null)
        {
            aimPivot = BuildAimIndicator();
        }
        WeaponSpriteRenderer = aimPivot.GetComponent<SpriteRenderer>();
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
        if (aimPivot == null || cam == null) return;

        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        Vector2 direction = mouseWorldPos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        aimPivot.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void BuildGlow()
    {
        GameObject glow = new GameObject("Glow");
        glow.transform.SetParent(transform, false);
        glow.transform.localPosition = new Vector3(0f, 0f, 0.1f);
        glow.transform.localScale = Vector3.one * 2.2f;

        SpriteRenderer glowSr = glow.AddComponent<SpriteRenderer>();
        glowSr.sprite = ProceduralSprites.SoftGlow;
        glowSr.sharedMaterial = ProceduralSprites.WorldSpriteMaterial;
        glowSr.color = new Color(0.35f, 0.85f, 1f, 0.35f);
        glowSr.sortingOrder = -1;
    }

    private Transform BuildAimIndicator()
    {
        GameObject indicator = new GameObject("AimIndicator");
        indicator.transform.SetParent(transform, false);
        indicator.transform.localPosition = Vector3.zero;
        indicator.transform.localScale = new Vector3(0.9f, 0.16f, 1f);

        SpriteRenderer indicatorSr = indicator.AddComponent<SpriteRenderer>();
        indicatorSr.sprite = ProceduralSprites.Square;
        indicatorSr.sharedMaterial = ProceduralSprites.WorldSpriteMaterial;
        indicatorSr.color = new Color(1f, 0.9f, 0.5f);
        indicatorSr.sortingOrder = 2;

        return indicator.transform;
    }
}
