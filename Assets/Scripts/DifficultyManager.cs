using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [Header("Dificultad inicial")]
    [SerializeField] private float startSpawnInterval = 2f;
    [SerializeField] private float startEnemySpeed = 2f;
    [SerializeField] private int startEnemyHealth = 30;
    [SerializeField] private int startEnemyDamage = 10;

    [Header("Límites (para que la dificultad no se vaya a extremos absurdos)")]
    [SerializeField] private float minSpawnInterval = 0.5f;
    [SerializeField] private float maxSpawnInterval = 4f;
    [SerializeField] private float maxEnemySpeed = 5f;
    [SerializeField] private int maxEnemyHealth = 100;

    [Header("Evaluación")]
    [SerializeField] private float evaluationInterval = 15f; // cada cuánto se reevalúa la dificultad

    // Parámetros de dificultad actuales - EnemySpawner y Enemy los leen desde acá
    public float SpawnInterval { get; private set; }
    public float EnemySpeed { get; private set; }
    public int EnemyHealth { get; private set; }
    public int EnemyContactDamage { get; private set; }

    // Métricas totales de la sesión
    private int kills;
    private int damageTaken;
    private int shotsFired;
    private int shotsHit;

    // Métricas acumuladas SOLO durante la ventana de evaluación actual
    private int killsThisWindow;
    private int damageTakenThisWindow;
    private float evaluationTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        SpawnInterval = startSpawnInterval;
        EnemySpeed = startEnemySpeed;
        EnemyHealth = startEnemyHealth;
        EnemyContactDamage = startEnemyDamage;
    }

    private void Update()
    {
        evaluationTimer += Time.deltaTime;
        if (evaluationTimer >= evaluationInterval)
        {
            EvaluateDifficulty();
            evaluationTimer = 0f;
        }
    }

    public void RegisterShotFired() => shotsFired++;

    public void RegisterShotHit() => shotsHit++;

    public void RegisterEnemyKilled()
    {
        kills++;
        killsThisWindow++;
    }

    public void RegisterPlayerDamaged(int amount)
    {
        damageTaken += amount;
        damageTakenThisWindow += amount;
    }

    private void EvaluateDifficulty()
    {
        // Regla baseline: si mataste mucho y recibiste poco daño, sube la dificultad.
        // Si recibiste mucho daño y mataste poco, bájala. Esto es lo que después
        // vamos a reemplazar (o complementar) con un modelo entrenado con datos reales.
        float performanceScore = killsThisWindow - (damageTakenThisWindow / 10f);

        if (performanceScore >= 5f)
        {
            IncreaseDifficulty();
        }
        else if (performanceScore <= 1f)
        {
            DecreaseDifficulty();
        }

        float accuracy = shotsFired > 0 ? (float)shotsHit / shotsFired : 0f;
        Debug.Log($"[Dificultad] Score ventana: {performanceScore} | Precisión total: {accuracy:P0} | SpawnInterval: {SpawnInterval:F2} | EnemySpeed: {EnemySpeed:F2} | EnemyHealth: {EnemyHealth}");

        killsThisWindow = 0;
        damageTakenThisWindow = 0;
    }

    private void IncreaseDifficulty()
    {
        SpawnInterval = Mathf.Max(minSpawnInterval, SpawnInterval * 0.85f);
        EnemySpeed = Mathf.Min(maxEnemySpeed, EnemySpeed * 1.1f);
        EnemyHealth = Mathf.Min(maxEnemyHealth, Mathf.RoundToInt(EnemyHealth * 1.15f));
    }

    private void DecreaseDifficulty()
    {
        SpawnInterval = Mathf.Min(maxSpawnInterval, SpawnInterval * 1.15f);
        EnemySpeed = Mathf.Max(1f, EnemySpeed * 0.9f);
        EnemyHealth = Mathf.Max(10, Mathf.RoundToInt(EnemyHealth * 0.9f));
    }
}
