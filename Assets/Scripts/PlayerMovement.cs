using UnityEngine;

// Shmup vertical: la nave se mueve libre con WASD dentro del área visible de la cámara (que hace
// scroll continuo hacia arriba). Ya no apunta con el mouse — dispara siempre hacia arriba
// (ver PlayerShooting), así que el indicador visual del arma también apunta fijo hacia arriba.
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float edgeMargin = 0.6f; // margen respecto al borde de la pantalla

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

        Transform aimPivot = BuildAimIndicator();
        WeaponSpriteRenderer = aimPivot.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        ReadMovementInput();
    }

    private void FixedUpdate()
    {
        // El movimiento físico va en FixedUpdate, no en Update, para que sea consistente sin importar el framerate
        Vector2 targetPos = rb.position + moveInput * moveSpeed * PerkEffects.MoveSpeedMultiplier * Time.fixedDeltaTime;
        rb.MovePosition(ClampToScreen(targetPos));
    }

    private void ReadMovementInput()
    {
        float x = Input.GetAxisRaw("Horizontal"); // A/D o flechas izquierda/derecha
        float y = Input.GetAxisRaw("Vertical");    // W/S o flechas arriba/abajo
        moveInput = new Vector2(x, y).normalized;   // normalizado para que la diagonal no sea más rápida
    }

    // La nave no puede salirse del área visible de la cámara, que a su vez hace scroll continuo
    // hacia arriba — así siempre estás "dentro de pantalla", como en un shmup vertical clásico.
    private Vector2 ClampToScreen(Vector2 pos)
    {
        if (cam == null) return pos;

        float halfHeight = Mathf.Max(0.5f, cam.orthographicSize - edgeMargin);
        float halfWidth = Mathf.Max(0.5f, halfHeight * cam.aspect - edgeMargin);
        Vector3 camPos = cam.transform.position;

        pos.x = Mathf.Clamp(pos.x, camPos.x - halfWidth, camPos.x + halfWidth);
        pos.y = Mathf.Clamp(pos.y, camPos.y - halfHeight, camPos.y + halfHeight);
        return pos;
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
        indicator.transform.localPosition = new Vector3(0f, 0.3f, 0f);
        indicator.transform.localScale = new Vector3(0.16f, 0.85f, 1f); // barra vertical: apunta fijo hacia arriba

        SpriteRenderer indicatorSr = indicator.AddComponent<SpriteRenderer>();
        indicatorSr.sprite = ProceduralSprites.Square;
        indicatorSr.sharedMaterial = ProceduralSprites.WorldSpriteMaterial;
        indicatorSr.color = new Color(1f, 0.9f, 0.5f);
        indicatorSr.sortingOrder = 2;

        return indicator.transform;
    }
}
