using UnityEngine;

// Compañero único que sigue al jugador y dispara solo al enemigo más cercano dentro de su rango.
// Solo puede haber uno a la vez: si ya tenés uno y recogés otro PetPickup, se le reinicia la
// duración en vez de sumar un segundo compañero (ver PetPickup.cs).
public class PetCompanion : MonoBehaviour
{
    public static PetCompanion Active { get; private set; }

    private const float Duration = 60f;
    private const float FadeWarningTime = 5f;
    private static readonly Color PetColor = new Color(0.85f, 0.4f, 1f);

    [SerializeField] private float followSpeed = 6f;
    [SerializeField] private float followDistance = 1.1f;
    [SerializeField] private float attackRange = 4.5f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private int damage = 6;
    [SerializeField] private float projectileSpeed = 10f;

    private Transform player;
    private SpriteRenderer sr;
    private float attackTimer;
    private float remainingDuration;

    private void Awake()
    {
        Active = this;
        remainingDuration = Duration;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        transform.position = player != null ? player.position + FollowOffset() : Vector3.zero;
        transform.localScale = Vector3.one * 0.5f;

        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = ProceduralSprites.Circle;
        sr.sharedMaterial = ProceduralSprites.WorldSpriteMaterial;
        sr.color = PetColor;
        sr.sortingOrder = 4;

        GameObject glow = new GameObject("Glow");
        glow.transform.SetParent(transform, false);
        glow.transform.localScale = Vector3.one * 2.6f;
        SpriteRenderer glowSr = glow.AddComponent<SpriteRenderer>();
        glowSr.sprite = ProceduralSprites.SoftGlow;
        glowSr.sharedMaterial = ProceduralSprites.WorldSpriteMaterial;
        glowSr.color = new Color(PetColor.r, PetColor.g, PetColor.b, 0.35f);
        glowSr.sortingOrder = -1;
    }

    private void OnDestroy()
    {
        if (Active == this) Active = null;
    }

    public void RefreshDuration()
    {
        remainingDuration = Duration;
    }

    private Vector3 FollowOffset() => new Vector3(-followDistance, followDistance, 0f);

    private void Update()
    {
        if (player == null) return;

        transform.position = Vector3.Lerp(transform.position, player.position + FollowOffset(), followSpeed * Time.deltaTime);

        remainingDuration -= Time.deltaTime;
        if (remainingDuration <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        if (remainingDuration <= FadeWarningTime)
        {
            float pulse = (Mathf.Sin(Time.time * 8f) + 1f) * 0.5f;
            sr.color = Color.Lerp(PetColor, Color.white, pulse * 0.6f);
        }

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        Transform closest = null;
        float closestDist = float.MaxValue;

        foreach (Collider2D col in hits)
        {
            if (!col.CompareTag("Enemy")) continue;
            float d = Vector2.Distance(transform.position, col.transform.position);
            if (d < closestDist)
            {
                closestDist = d;
                closest = col.transform;
            }
        }

        if (closest == null) return;

        Vector2 dir = ((Vector2)closest.position - (Vector2)transform.position).normalized;
        int finalDamage = Mathf.RoundToInt(damage * PerkEffects.PetDamageMultiplier);

        GameObject shotObj = new GameObject("PetShot");
        shotObj.transform.position = transform.position;
        shotObj.AddComponent<PetProjectile>().Init(dir, projectileSpeed, finalDamage);

        attackTimer = attackCooldown;
    }
}
