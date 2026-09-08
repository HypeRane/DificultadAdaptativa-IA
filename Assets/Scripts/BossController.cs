using UnityEngine;

// Comportamiento extra de los jefes, además del Enemy base que llevan pegado (vida, daño de
// contacto y muerte ya los resuelve Enemy). Agrega el patrón de disparo radial y avisa
// globalmente cuando un jefe muere, para que PerkManager ofrezca una mejora.
public class BossController : MonoBehaviour
{
    public static BossController ActiveBoss { get; private set; }
    public static event System.Action OnAnyBossDefeated;

    [SerializeField] private float burstInterval = 3.5f;
    [SerializeField] private int burstProjectileCount = 10;
    [SerializeField] private int burstDamage = 8;
    [SerializeField] private float burstProjectileSpeed = 5f;

    [Header("Andanada dirigida (reacciona a que el jugador campee)")]
    [SerializeField] private float focusedVolleyInterval = 5f;
    [SerializeField] private int focusedVolleyCount = 4;
    [SerializeField] private float focusedVolleySpreadDegrees = 14f;

    public string DisplayName { get; private set; } = "JEFE";

    private Enemy enemyPart;
    private Transform player;
    private float burstTimer;
    private float focusedVolleyTimer;

    private void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    public void Init(Enemy enemy, string displayName)
    {
        enemyPart = enemy;
        DisplayName = displayName;
        enemyPart.OnDied += HandleDefeated;
        burstTimer = burstInterval * 0.5f;
        focusedVolleyTimer = focusedVolleyInterval * 0.5f;
        ActiveBoss = this;
    }

    private void OnDestroy()
    {
        if (ActiveBoss == this) ActiveBoss = null;
        if (enemyPart != null) enemyPart.OnDied -= HandleDefeated;
    }

    private void Update()
    {
        burstTimer -= Time.deltaTime;
        if (burstTimer <= 0f)
        {
            burstTimer = burstInterval;
            FireRadialBurst();
        }

        focusedVolleyTimer -= Time.deltaTime;
        if (focusedVolleyTimer <= 0f)
        {
            focusedVolleyTimer = focusedVolleyInterval;
            TryFireFocusedVolley();
        }
    }

    // Si el jugador está campeando, el jefe manda una andanada apuntada directo a su posición
    // (además del estallido radial de siempre), para que esconderse en un punto fijo no alcance.
    private void TryFireFocusedVolley()
    {
        if (player == null) return;
        if (PlayerTelemetryTracker.Instance == null || !PlayerTelemetryTracker.Instance.IsPlayerCamping) return;

        SoundManager.Play(Sfx.BossAlarm, 0.5f);

        Vector2 baseDir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;

        for (int i = 0; i < focusedVolleyCount; i++)
        {
            float t = focusedVolleyCount > 1 ? (float)i / (focusedVolleyCount - 1) - 0.5f : 0f;
            float angle = baseAngle + t * focusedVolleySpreadDegrees;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            GameObject bulletObj = new GameObject("BossFocusedBullet");
            bulletObj.transform.position = transform.position;
            bulletObj.AddComponent<EnemyProjectile>().Init(dir, burstProjectileSpeed * 1.3f, burstDamage, new Color(1f, 0.7f, 0.1f));
        }
    }

    private void FireRadialBurst()
    {
        SoundManager.Play(Sfx.BossAlarm, 0.6f);

        for (int i = 0; i < burstProjectileCount; i++)
        {
            float angle = (360f / burstProjectileCount) * i;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            GameObject bulletObj = new GameObject("BossBullet");
            bulletObj.transform.position = transform.position;
            bulletObj.AddComponent<EnemyProjectile>().Init(dir, burstProjectileSpeed, burstDamage, new Color(1f, 0.2f, 0.5f));
        }
    }

    private void HandleDefeated()
    {
        GameManager.Instance?.RegisterBossBonus(transform.position);
        OnAnyBossDefeated?.Invoke();
    }
}
