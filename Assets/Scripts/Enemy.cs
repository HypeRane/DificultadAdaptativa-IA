using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private float moveSpeed;
    private int maxHealth;
    private int contactDamage;

    private int currentHealth;
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Color baseColor;
    private Coroutine flashRoutine;

    private bool isRanged;
    private float preferredDistance;
    private float rangedFireInterval;
    private int rangedDamage;
    private float rangedProjectileSpeed;
    private float rangedAttackTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        sr = GetComponent<SpriteRenderer>();
        if (sr != null) baseColor = sr.color;

        // Lee las estadísticas actuales desde el sistema de dificultad.
        // Si por algún motivo no existe en la escena, usa valores por defecto para no romper el juego.
        if (DifficultyManager.Instance != null)
        {
            moveSpeed = DifficultyManager.Instance.EnemySpeed;
            maxHealth = DifficultyManager.Instance.EnemyHealth;
            contactDamage = DifficultyManager.Instance.EnemyContactDamage;
        }
        else
        {
            moveSpeed = 2f;
            maxHealth = 30;
            contactDamage = 10;
        }

        currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    // Aplica un arquetipo (Rápido/Bruto/Tirador/...) sobre las stats base ya leídas de
    // DifficultyManager. Debe llamarse antes de BeginSpawnAnimation.
    public void ApplyArchetype(EnemyKind kind)
    {
        EnemyArchetype arch = EnemyDatabase.All[kind];

        moveSpeed *= arch.SpeedMultiplier;
        maxHealth = Mathf.Max(1, Mathf.RoundToInt(maxHealth * arch.HealthMultiplier));
        contactDamage = Mathf.Max(1, Mathf.RoundToInt(contactDamage * arch.DamageMultiplier));
        currentHealth = maxHealth;

        transform.localScale *= arch.ScaleMultiplier;

        if (sr != null)
        {
            sr.color = arch.Color;
            baseColor = arch.Color;
        }

        isRanged = arch.IsRanged;
        preferredDistance = arch.PreferredDistance;
        rangedFireInterval = arch.RangedFireInterval;
        rangedDamage = arch.RangedDamage;
        rangedProjectileSpeed = arch.RangedProjectileSpeed;
        rangedAttackTimer = Random.Range(0f, rangedFireInterval); // para que no disparen todos sincronizados
    }

    public void BeginSpawnAnimation()
    {
        StartCoroutine(SpawnInRoutine());
    }

    private IEnumerator SpawnInRoutine()
    {
        Vector3 targetScale = transform.localScale;
        transform.localScale = Vector3.zero;
        float t = 0f;
        const float duration = 0.25f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / duration);
            transform.localScale = targetScale * p;
            yield return null;
        }
        transform.localScale = targetScale;
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        Vector2 toPlayer = (Vector2)player.position - rb.position;
        Vector2 dir = toPlayer.normalized;

        if (isRanged)
        {
            float dist = toPlayer.magnitude;
            Vector2 moveDir = Vector2.zero;
            if (dist > preferredDistance + 0.5f) moveDir = dir;
            else if (dist < preferredDistance - 0.5f) moveDir = -dir;
            rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
        }
    }

    private void Update()
    {
        if (!isRanged || player == null) return;

        rangedAttackTimer -= Time.deltaTime;
        if (rangedAttackTimer <= 0f)
        {
            rangedAttackTimer = rangedFireInterval;
            FireAtPlayer();
        }
    }

    private void FireAtPlayer()
    {
        Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        GameObject bulletObj = new GameObject("EnemyBullet");
        bulletObj.transform.position = transform.position;
        bulletObj.AddComponent<EnemyProjectile>().Init(dir, rangedProjectileSpeed, rangedDamage, baseColor);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        FlashHit();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void FlashHit()
    {
        if (sr == null) return;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        sr.color = Color.white;
        yield return new WaitForSeconds(0.06f);
        sr.color = baseColor;
    }

    private void Die()
    {
        DifficultyManager.Instance?.RegisterEnemyKilled();
        GameManager.Instance?.RegisterKill(transform.position);
        HitEffects.SpawnBurst(transform.position, baseColor, 10, 5f, 0.45f);
        CameraFollow.Shake(0.12f, 0.08f);
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage);
            }
        }
    }
}
