using System;
using System.IO;
using UnityEngine;

// Ritmo del director adaptativo. Valley/Falling bajan la presión (recuperación), Rising la
// sube gradualmente, Climax la lleva al pico por un tiempo acotado. PlayerTelemetryTracker
// alimenta el SkillFactor que decide cuándo transicionar entre estados.
public enum DirectorState
{
    Valley,
    Rising,
    Climax,
    Falling
}

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
    [SerializeField] private float evaluationInterval = 15f; // cada cuánto se reevalúa la dificultad base (progresión lenta)

    [Header("Director de ritmo (Tensión/Valle/Clímax)")]
    [SerializeField] private float stateCheckInterval = 2f;   // cada cuánto se revisan transiciones (no cada frame)
    [SerializeField] private float valleyMinDuration = 10f;
    [SerializeField] private float risingDuration = 20f;
    [SerializeField] private float climaxDuration = 25f;
    [SerializeField] private float fallingDuration = 12f;

    // Parámetros de dificultad actuales - EnemySpawner y Enemy los leen desde acá
    public float SpawnInterval { get; private set; }
    public float EnemySpeed { get; private set; }
    public int EnemyHealth { get; private set; }
    public int EnemyContactDamage { get; private set; }

    // Nivel demostrativo (1-10) para mostrar en el HUD, junto con un evento para avisar cambios
    public int DifficultyLevel { get; private set; } = 1;
    public float Accuracy => shotsFired > 0 ? (float)shotsHit / shotsFired : 0f;
    public int TotalKills => kills;

    // --- Director de ritmo: capa reactiva en tiempo real, además de la progresión lenta de arriba ---
    public DirectorState CurrentState { get; private set; } = DirectorState.Valley;
    public float SkillFactor { get; private set; } = 0.5f; // lo reporta PlayerTelemetryTracker, ya suavizado
    public float SpawnIntervalMultiplier { get; private set; } = 1f; // EnemySpawner lo aplica encima de SpawnInterval

    public event Action<bool> OnDifficultyChanged; // true = subió, false = bajó (progresión lenta)
    public event Action<DirectorState> OnStateChanged; // cambios del director de ritmo

    // Métricas totales de la sesión
    private int kills;
    private int damageTaken;
    private int shotsFired;
    private int shotsHit;

    // Métricas acumuladas SOLO durante la ventana de evaluación actual
    private int killsThisWindow;
    private int damageTakenThisWindow;
    private float evaluationTimer;

    private float stateCheckTimer;
    private float stateTimeInState;

    private string logFilePath;

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

        SetupLogFile();
    }

    private void SetupLogFile()
    {
        logFilePath = Path.Combine(Application.persistentDataPath, "difficulty_sessions.csv");

        if (!File.Exists(logFilePath))
        {
            string header = "timestamp,elapsed_seconds,kills_window,damage_taken_window,shots_fired_total,shots_hit_total,accuracy_total,performance_score,spawn_interval,enemy_speed,enemy_health,director_state,skill_factor\n";
            File.WriteAllText(logFilePath, header);
        }

        Debug.Log($"[Dificultad] Guardando datos de sesión en: {logFilePath}");
    }

    private void Update()
    {
        evaluationTimer += Time.deltaTime;
        if (evaluationTimer >= evaluationInterval)
        {
            EvaluateDifficulty();
            evaluationTimer = 0f;
        }

        stateCheckTimer += Time.deltaTime;
        if (stateCheckTimer >= stateCheckInterval)
        {
            stateCheckTimer = 0f;
            UpdateDirectorState();
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

    // Llamado por PlayerTelemetryTracker cada vez que recalcula el SkillFactor suavizado
    // (precisión + salud + ritmo de kills + movilidad). El director de ritmo lo usa para decidir
    // transiciones de estado, y también refina la evaluación de progresión lenta de más abajo.
    public void ReportSkillFactor(float value)
    {
        SkillFactor = Mathf.Clamp01(value);
    }

    private void EvaluateDifficulty()
    {
        // Regla baseline: si mataste mucho y recibiste poco daño, sube la dificultad.
        // Si recibiste mucho daño y mataste poco, bájala. El SkillFactor (telemetría más rica:
        // precisión, salud, ritmo de kills, movilidad) afina esta señal cruda sin reemplazarla.
        float performanceScore = killsThisWindow - (damageTakenThisWindow / 10f);
        float blendedScore = performanceScore + (SkillFactor - 0.5f) * 6f;

        if (blendedScore >= 5f)
        {
            IncreaseDifficulty();
        }
        else if (blendedScore <= 1f)
        {
            DecreaseDifficulty();
        }

        float accuracy = shotsFired > 0 ? (float)shotsHit / shotsFired : 0f;
        Debug.Log($"[Dificultad] Score ventana: {performanceScore} (blend {blendedScore:F1}) | Precisión total: {accuracy:P0} | Estado: {CurrentState} | SkillFactor: {SkillFactor:F2} | SpawnInterval: {SpawnInterval:F2} | EnemySpeed: {EnemySpeed:F2} | EnemyHealth: {EnemyHealth}");

        LogEvaluation(performanceScore, accuracy);

        killsThisWindow = 0;
        damageTakenThisWindow = 0;
    }

    private void LogEvaluation(float performanceScore, float accuracy)
    {
        string row = $"{DateTime.Now:O},{Time.time:F1},{killsThisWindow},{damageTakenThisWindow},{shotsFired},{shotsHit},{accuracy:F2},{performanceScore:F2},{SpawnInterval:F2},{EnemySpeed:F2},{EnemyHealth},{CurrentState},{SkillFactor:F2}\n";

        try
        {
            File.AppendAllText(logFilePath, row);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"No se pudo escribir el log de dificultad: {e.Message}");
        }
    }

    private void IncreaseDifficulty()
    {
        // El piso efectivo de intervalo se sube un poco a propósito (por encima de minSpawnInterval)
        // para que los enemigos no se amontonen tan rápido, aunque el Inspector tenga un valor menor.
        float effectiveMinInterval = Mathf.Max(minSpawnInterval, 0.75f);
        SpawnInterval = Mathf.Max(effectiveMinInterval, SpawnInterval * 0.9f);
        EnemySpeed = Mathf.Min(maxEnemySpeed, EnemySpeed * 1.1f);
        EnemyHealth = Mathf.Min(maxEnemyHealth, Mathf.RoundToInt(EnemyHealth * 1.12f));

        DifficultyLevel = Mathf.Min(10, DifficultyLevel + 1);
        OnDifficultyChanged?.Invoke(true);
    }

    private void DecreaseDifficulty()
    {
        SpawnInterval = Mathf.Min(maxSpawnInterval, SpawnInterval * 1.15f);
        EnemySpeed = Mathf.Max(1f, EnemySpeed * 0.9f);
        EnemyHealth = Mathf.Max(10, Mathf.RoundToInt(EnemyHealth * 0.9f));

        DifficultyLevel = Mathf.Max(1, DifficultyLevel - 1);
        OnDifficultyChanged?.Invoke(false);
    }

    // --- Director de ritmo ---
    // Máquina de estados chica y explícita (sin tablas ni reflexión) para mantenerla legible.
    // Corre cada stateCheckInterval segundos, no cada frame.
    private void UpdateDirectorState()
    {
        stateTimeInState += stateCheckInterval;

        switch (CurrentState)
        {
            case DirectorState.Valley:
                if (stateTimeInState >= valleyMinDuration && SkillFactor >= 0.55f)
                {
                    TransitionTo(DirectorState.Rising);
                }
                break;

            case DirectorState.Rising:
                if (SkillFactor <= 0.25f)
                {
                    TransitionTo(DirectorState.Falling); // el jugador está sufriendo: aliviar ya
                }
                else if (stateTimeInState >= risingDuration || SkillFactor >= 0.8f)
                {
                    TransitionTo(DirectorState.Climax);
                }
                break;

            case DirectorState.Climax:
                if (SkillFactor <= 0.25f || stateTimeInState >= climaxDuration)
                {
                    TransitionTo(DirectorState.Falling);
                }
                break;

            case DirectorState.Falling:
                if (stateTimeInState >= fallingDuration)
                {
                    TransitionTo(DirectorState.Valley);
                }
                break;
        }

        float targetMultiplier = GetTargetSpawnMultiplier(CurrentState);
        SpawnIntervalMultiplier = Mathf.Lerp(SpawnIntervalMultiplier, targetMultiplier, 0.3f);
    }

    private void TransitionTo(DirectorState newState)
    {
        CurrentState = newState;
        stateTimeInState = 0f;
        OnStateChanged?.Invoke(newState);
    }

    private float GetTargetSpawnMultiplier(DirectorState state)
    {
        switch (state)
        {
            case DirectorState.Valley: return 1.35f;  // respiro: aparecen más espaciados
            case DirectorState.Rising: return 1f;      // ritmo base
            case DirectorState.Climax: return 0.65f;   // pico: aparecen bastante más seguido
            case DirectorState.Falling: return 1.1f;   // empieza a aflojar
            default: return 1f;
        }
    }
}
