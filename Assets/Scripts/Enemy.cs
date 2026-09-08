using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Ambiguo con System.Random si se agrega "using System;" acá (el archivo ya usa
    // Random.Range de UnityEngine sin calificar) — se deja el evento con el tipo completo.
    public event System.Action OnDied;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private float moveSpeed;
    private int maxHealth;
    private int contactDamage;

    private int currentHealth;
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Camera mainCam;
    private Color baseColor;
    private Coroutine flashRoutine;

    private bool isRanged;
    private float preferredDistance;
    private float rangedFireInterval;
    private int rangedDamage;
    private float rangedProjectileSpeed;
    private float rangedAttackTimer;

    private bool isBomber;
    private float bomberExplosionRadius;
    private int bomberExplosionDamage;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        mainCam = Camera.main;

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

        isBomber = arch.IsBomber;
        bomberExplosionRadius = arch.BomberExplosionRadius;
        bomberExplosionDamage = arch.BomberExplosionDamage;
    }

    // Configura al jefe con stats propias (mucho más grandes que un enemigo común) sobre el
    // mismo componente Enemy, para heredar gratis toda la lógica de vida/daño/muerte existente.
    public void ConfigureAsBoss(int bossMaxHealth, float bossSpeed, int bossContactDamage, Color color)
    {
        maxHealth = bossMaxHealth;
        currentHealth = maxHealth;
        moveSpeed = bossSpeed;
        contactDamage = bossContactDamage;

        if (sr != null)
        {
            sr.color = color;
            baseColor = color;
        }
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
        CheckOffscreenCleanup();

        if (!isRanged || player == null) return;

        rangedAttackTimer -= Time.deltaTime;
        if (rangedAttackTimer <= 0f)
        {
            rangedAttackTimer = rangedFireInterval;
            FireAtPlayer();
        }
    }

    // La cámara del shmup vertical hace scroll continuo hacia arriba: si el jugador dejó a este
    // enemigo (o al jefe, que también usa este componente) muy atrás, ya no tiene sentido seguir
    // persiguiendo. Se limpia directo con Destroy (no Die()) para no dar puntaje ni disparar el
    // evento OnDied — no fue derrotado, solo quedó fuera de pantalla.
    private void CheckOffscreenCleanup()
    {
        if (mainCam == null) return;

        float cleanupY = mainCam.transform.position.y - mainCam.orthographicSize * 2.5f;
        if (transform.position.y < cleanupY)
        {
            Destroy(gameObject);
        }
    }

    private void FireAtPlayer()
    {
        Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;

        // En Clímax el director sube ligeramente la velocidad de los proyectiles (sin tocar
        // EnemyProjectile ni sus colliders: solo pasa un valor distinto al Init existente).
        float speed = rangedProjectileSpeed;
        if (DifficultyManager.Instance != null && DifficultyManager.Instance.CurrentState == DirectorState.Climax)
        {
            speed *= 1.15f;
        }

        bool campingDetected = PlayerTelemetryTracker.Instance != null && PlayerTelemetryTracker.Instance.IsPlayerCamping;
        if (campingDetected)
        {
            FireFan(dir, speed);
        }
        else
        {
            SpawnBullet(dir, speed);
        }
    }

    // Si el jugador se queda quieto en un punto "seguro", el Escupidor abre el abanico de tiro
    // para que ese punto deje de ser seguro, en vez de seguir mandando un solo disparo directo.
    private void FireFan(Vector2 baseDir, float speed)
    {
        const int fanShots = 3;
        const float fanSpreadDegrees = 22f;
        float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;

        for (int i = 0; i < fanShots; i++)
        {
            float t = (float)i / (fanShots - 1) - 0.5f;
            float angle = baseAngle + t * fanSpreadDegrees;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            SpawnBullet(dir, speed);
        }
    }

    private void SpawnBullet(Vector2 dir, float speed)
    {
        GameObject bulletObj = new GameObject("EnemyBullet");
        bulletObj.transform.position = transform.position;
        bulletObj.AddComponent<EnemyProjectile>().Init(dir, speed, rangedDamage, baseColor);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        SoundManager.Play(Sfx.Hit, 0.5f, Random.Range(0.9f, 1.1f));
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
        SoundManager.Play(Sfx.EnemyDeath);
        OnDied?.Invoke();

        if (isBomber)
        {
            DealExplosionDamage();
            HitEffects.SpawnBurst(transform.position, baseColor, 14, 5f, 0.4f);
            SoundManager.Play(Sfx.Explosion);
            CameraFollow.Shake(0.2f, 0.16f);
        }
        else
        {
            HitEffects.SpawnBurst(transform.position, baseColor, 10, 5f, 0.45f);
            CameraFollow.Shake(0.12f, 0.08f);
        }

        Destroy(gameObject);
    }

    // El Explosivo no reparte daño de contacto normal: detona en área al llegar al jugador
    // (o al morir por cualquier otra causa), dañando solo si el jugador quedó dentro del radio.
    private void DealExplosionDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, bomberExplosionRadius);
        foreach (Collider2D col in hits)
        {
            if (!col.CompareTag("Player")) continue;
            PlayerHealth playerHealth = col.GetComponent<PlayerHealth>();
            if (playerHealth != null) playerHealth.TakeDamage(bomberExplosionDamage);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        if (isBomber)
        {
            Die();
            return;
        }

        PlayerHealth normalHit = collision.gameObject.GetComponent<PlayerHealth>();
        if (normalHit != null)
        {
            normalHit.TakeDamage(contactDamage);
        }
    }
}
