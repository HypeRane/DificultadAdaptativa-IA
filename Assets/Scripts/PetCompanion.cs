using System.Collections.Generic;
using UnityEngine;

// Dron que orbita al jugador y dispara solo al enemigo más cercano dentro de su rango.
// Se pueden tener hasta MaxDrones a la vez; si ya estás en el máximo y recogés otro
// PetPickup, se les reinicia la duración a todos en vez de sumar uno nuevo.
public class PetCompanion : MonoBehaviour
{
    public const int MaxDrones = 3;
    private static readonly List<PetCompanion> Active = new List<PetCompanion>();
    public static int ActiveCount => Active.Count;

    private const float Duration = 60f;
    private const float FadeWarningTime = 5f;
    private static readonly Color DroneColor = new Color(0.85f, 0.4f, 1f);

    [SerializeField] private float followSpeed = 6f;
    [SerializeField] private float orbitRadius = 1.3f;
    [SerializeField] private float attackRange = 4.5f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private int damage = 6;
    [SerializeField] private float projectileSpeed = 10f;

    private Transform player;
    private SpriteRenderer sr;
    private float attackTimer;
    private float remainingDuration;
    private float orbitAngleOffset;

    private void Awake()
    {
        Active.Add(this);
        orbitAngleOffset = (Active.IndexOf(this) * 137.5f) % 360f; // ángulo dorado: se reparten solas sin amontonarse
        remainingDuration = Duration;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        transform.position = player != null ? player.position : Vector3.zero;
        transform.localScale = Vector3.one * 0.5f;

        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = ProceduralSprites.Circle;
        sr.sharedMaterial = ProceduralSprites.WorldSpriteMaterial;
        sr.color = DroneColor;
        sr.sortingOrder = 4;

        GameObject glow = new GameObject("Glow");
        glow.transform.SetParent(transform, false);
        glow.transform.localScale = Vector3.one * 2.6f;
        SpriteRenderer glowSr = glow.AddComponent<SpriteRenderer>();
        glowSr.sprite = ProceduralSprites.SoftGlow;
        glowSr.sharedMaterial = ProceduralSprites.WorldSpriteMaterial;
        glowSr.color = new Color(DroneColor.r, DroneColor.g, DroneColor.b, 0.35f);
        glowSr.sortingOrder = -1;
    }

    private void OnDestroy()
    {
        Active.Remove(this);
    }

    public static void RefreshAll()
    {
        foreach (PetCompanion drone in Active)
        {
            drone.remainingDuration = Duration;
        }
    }

    private void Update()
    {
        if (player == null) return;

        float angle = (Time.time * 40f + orbitAngleOffset) * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * orbitRadius;
        transform.position = Vector3.Lerp(transform.position, player.position + offset, followSpeed * Time.deltaTime);

        remainingDuration -= Time.deltaTime;
        if (remainingDuration <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        if (remainingDuration <= FadeWarningTime)
        {
            float pulse = (Mathf.Sin(Time.time * 8f) + 1f) * 0.5f;
            sr.color = Color.Lerp(DroneColor, Color.white, pulse * 0.6f);
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
