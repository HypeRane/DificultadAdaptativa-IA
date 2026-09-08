using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f; // segundos antes de autodestruirse si no choca con nada
    [SerializeField] private int damage = 10;

    private Vector2 direction;
    private float speed;
    private bool isExplosive;
    private float explosionRadius;
    private bool hasFalloff;
    private float falloffStart;
    private float falloffEnd;
    private float minDamageMultiplier = 1f;
    private Vector3 spawnPosition;

    public void SetDirection(Vector2 newDirection, float newSpeed)
    {
        direction = newDirection;
        speed = newSpeed;
    }

    // Permite que PlayerShooting adapte el proyectil según el arma actual (daño, color, si explota,
    // y si pierde fuerza con la distancia recorrida, como la escopeta).
    public void Configure(int newDamage, Color color, bool explosive, float radius,
        bool falloff = false, float falloffStartRange = 0f, float falloffEndRange = 0f, float minDamageMult = 1f)
    {
        damage = newDamage;
        isExplosive = explosive;
        explosionRadius = radius;
        hasFalloff = falloff;
        falloffStart = falloffStartRange;
        falloffEnd = falloffEndRange;
        minDamageMultiplier = minDamageMult;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = color;
    }

    private void Start()
    {
        spawnPosition = transform.position;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (isExplosive)
            {
                Explode();
            }
            else
            {
                Enemy enemy = other.GetComponent<Enemy>();
                if (enemy != null)
                {
                    int finalDamage = hasFalloff ? ApplyFalloff(damage) : damage;
                    enemy.TakeDamage(finalDamage);
                    DifficultyManager.Instance?.RegisterShotHit();
                    HitEffects.SpawnBurst(transform.position, Color.white, 4, 3f, 0.25f);
                }
            }
            Destroy(gameObject);
            return;
        }

        if (other.GetComponent<ObstacleMarker>() != null)
        {
            if (isExplosive) Explode();
            HitEffects.SpawnBurst(transform.position, new Color(0.5f, 0.45f, 0.4f), 5, 2.5f, 0.2f);
            Destroy(gameObject);
        }
    }

    private void Explode()
    {
        CameraFollow.Shake(0.2f, 0.18f);
        HitEffects.SpawnBurst(transform.position, new Color(1f, 0.6f, 0.1f), 16, 6f, 0.5f);
        SoundManager.Play(Sfx.Explosion);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        bool hitAny = false;
        foreach (Collider2D col in hits)
        {
            if (!col.CompareTag("Enemy")) continue;
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                hitAny = true;
            }
        }

        if (hitAny) DifficultyManager.Instance?.RegisterShotHit();
    }

    private int ApplyFalloff(int baseDamage)
    {
        float traveled = Vector2.Distance(spawnPosition, transform.position);
        float t = Mathf.InverseLerp(falloffStart, falloffEnd, traveled);
        float mult = Mathf.Lerp(1f, minDamageMultiplier, t);
        return Mathf.Max(1, Mathf.RoundToInt(baseDamage * mult));
    }
}
