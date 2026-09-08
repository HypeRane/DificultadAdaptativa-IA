using UnityEngine;

// Capa de telemetría del jugador: computa un SkillFactor (0..1) suavizado a partir de precisión,
// salud actual, ritmo de eliminaciones y movilidad (cuánto usa el mapa vs. si campea quieto en
// un punto). Sondea sobre una ventana móvil y se lo reporta a DifficultyManager, que es quien
// decide qué hacer con esa señal (ver DifficultyManager.ReportSkillFactor).
//
// Cero asignaciones en Update: nada de LINQ, nada de List<> creciendo con el tiempo. El historial
// de posiciones usa un buffer circular de tamaño fijo (SampleCount), y el resto son floats/ints.
public class PlayerTelemetryTracker : MonoBehaviour
{
    public static PlayerTelemetryTracker Instance { get; private set; }

    public float SkillFactor { get; private set; } = 0.5f;
    public bool IsPlayerCamping { get; private set; }

    [Header("Muestreo (ventana móvil)")]
    [SerializeField] private float sampleInterval = 1.5f;   // cada cuánto se toma una muestra de posición
    [SerializeField] private float windowSeconds = 12f;      // cada cuánto se recalcula el SkillFactor
    [SerializeField] private float smoothing = 0.35f;        // 0=nunca cambia, 1=salta directo al valor crudo

    [Header("Movilidad / detección de camping")]
    [SerializeField] private float goodMobilitySpeed = 1.8f; // unidades/seg promedio que cuentan como "usa el mapa"
    [SerializeField] private float campingMobilityThreshold = 0.25f; // por debajo de este score de movilidad, se considera camping

    private const int SampleCount = 8; // buffer circular fijo: (SampleCount-1)*sampleInterval ≈ ventana cubierta
    private readonly Vector2[] positionSamples = new Vector2[SampleCount];
    private int sampleWriteIndex;
    private int samplesFilled;
    private float sampleTimer;
    private float windowTimer;

    private Transform player;
    private PlayerHealth playerHealth;
    private int killsAtWindowStart;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }
    }

    private void Update()
    {
        if (player == null) return;

        sampleTimer += Time.deltaTime;
        if (sampleTimer >= sampleInterval)
        {
            sampleTimer = 0f;
            TakeSample();
        }

        windowTimer += Time.deltaTime;
        if (windowTimer >= windowSeconds)
        {
            windowTimer = 0f;
            EvaluateWindow();
        }
    }

    private void TakeSample()
    {
        positionSamples[sampleWriteIndex] = player.position;
        sampleWriteIndex = (sampleWriteIndex + 1) % SampleCount;
        if (samplesFilled < SampleCount) samplesFilled++;
    }

    private void EvaluateWindow()
    {
        float mobilityScore = ComputeMobilityScore();
        float accuracyScore = DifficultyManager.Instance != null ? DifficultyManager.Instance.Accuracy : 0.5f;
        float healthScore = playerHealth != null && playerHealth.MaxHealth > 0
            ? (float)playerHealth.CurrentHealth / playerHealth.MaxHealth
            : 1f;
        float killRateScore = ComputeKillRateScore();

        float raw = Mathf.Clamp01(
            accuracyScore * 0.3f +
            healthScore * 0.3f +
            killRateScore * 0.25f +
            mobilityScore * 0.15f);

        SkillFactor = Mathf.Lerp(SkillFactor, raw, smoothing);
        IsPlayerCamping = mobilityScore < campingMobilityThreshold;

        DifficultyManager.Instance?.ReportSkillFactor(SkillFactor);
    }

    // Recorre el buffer circular a mano (sin LINQ) sumando la distancia entre muestras
    // consecutivas, para estimar qué tan quieto/móvil estuvo el jugador en la ventana.
    private float ComputeMobilityScore()
    {
        if (samplesFilled < 2) return 0.5f; // todavía no hay datos suficientes: neutral

        float totalDistance = 0f;
        for (int i = 1; i < samplesFilled; i++)
        {
            int a = (sampleWriteIndex - i + SampleCount) % SampleCount;
            int b = (sampleWriteIndex - i - 1 + SampleCount) % SampleCount;
            totalDistance += Vector2.Distance(positionSamples[a], positionSamples[b]);
        }

        float elapsed = (samplesFilled - 1) * sampleInterval;
        float avgSpeed = elapsed > 0f ? totalDistance / elapsed : 0f;

        return Mathf.Clamp01(avgSpeed / goodMobilitySpeed);
    }

    private float ComputeKillRateScore()
    {
        if (GameManager.Instance == null) return 0.5f;

        int killsNow = GameManager.Instance.KillCount;
        int killsInWindow = killsNow - killsAtWindowStart;
        killsAtWindowStart = killsNow;

        const float expectedKillsPerWindow = 4f; // referencia aproximada para un ritmo "normal"
        return Mathf.Clamp01(killsInWindow / expectedKillsPerWindow);
    }
}
