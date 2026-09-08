using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// Lleva el puntaje, el combo y el estado de partida. Es lo que convierte "sobrevivir" en un juego con objetivo.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private const float ComboWindowSeconds = 2.5f;
    private const int BaseKillScore = 10;

    public int Score { get; private set; }
    public int Combo { get; private set; }
    public int MaxCombo { get; private set; }
    public int KillCount { get; private set; }
    public float SurvivalTime { get; private set; }
    public bool IsGameOver { get; private set; }

    public event Action<int, int> OnScoreChanged; // score, combo
    public event Action<bool> OnGameOver; // true = nuevo récord de puntaje
    public event Action<Vector3> OnComboMilestone; // posición donde ocurrió el kill que disparó el hito

    private const int ComboMilestoneStep = 8;
    private const int BossKillBonus = 500;

    private float comboTimer;
    private PlayerHealth cachedPlayerHealth;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        PerkEffects.Reset(); // cada partida nueva arranca sin las mejoras de la run anterior
    }

    private void Update()
    {
        if (IsGameOver) return;

        SurvivalTime += Time.deltaTime;

        if (Combo > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
            {
                Combo = 0;
                OnScoreChanged?.Invoke(Score, Combo);
            }
        }
    }

    public void RegisterKill(Vector3 position)
    {
        if (IsGameOver) return;

        KillCount++;
        Combo++;
        if (Combo > MaxCombo) MaxCombo = Combo;
        comboTimer = ComboWindowSeconds;

        int points = Mathf.RoundToInt(BaseKillScore * Combo * PerkEffects.ScoreMultiplier);
        Score += points;

        OnScoreChanged?.Invoke(Score, Combo);

        Color textColor = Combo > 1 ? new Color(1f, 0.75f, 0.2f) : Color.white;
        HUDController.Instance?.ShowFloatingText(position, $"+{points}", textColor, Combo > 1 ? 32f : 24f);

        if (PerkEffects.LifeStealPerKill > 0f)
        {
            ApplyLifeSteal();
        }

        if (Combo > 0 && Combo % ComboMilestoneStep == 0)
        {
            OnComboMilestone?.Invoke(position);
        }
    }

    // Bonificación extra al derrotar un jefe, además del puntaje normal por la eliminación.
    public void RegisterBossBonus(Vector3 position)
    {
        if (IsGameOver) return;

        Score += BossKillBonus;
        OnScoreChanged?.Invoke(Score, Combo);
        HUDController.Instance?.ShowFloatingText(position, $"+{BossKillBonus} ¡JEFE!", new Color(1f, 0.3f, 0.3f), 40f);
    }

    private void ApplyLifeSteal()
    {
        if (cachedPlayerHealth == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) cachedPlayerHealth = playerObj.GetComponent<PlayerHealth>();
        }
        cachedPlayerHealth?.Heal(Mathf.RoundToInt(PerkEffects.LifeStealPerKill));
    }

    public void TriggerGameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;

        int level = DifficultyManager.Instance != null ? DifficultyManager.Instance.DifficultyLevel : 1;
        bool isNewRecord = HighScoreManager.SubmitRun(Score, MaxCombo, level, SurvivalTime);

        SoundManager.Play(Sfx.GameOver);
        OnGameOver?.Invoke(isNewRecord);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
