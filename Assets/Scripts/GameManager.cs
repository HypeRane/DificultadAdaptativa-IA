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
    public int KillCount { get; private set; }
    public float SurvivalTime { get; private set; }
    public bool IsGameOver { get; private set; }

    public event Action<int, int> OnScoreChanged; // score, combo
    public event Action OnGameOver;
    public event Action<Vector3> OnComboMilestone; // posición donde ocurrió el kill que disparó el hito

    private const int ComboMilestoneStep = 8;

    private float comboTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
        comboTimer = ComboWindowSeconds;

        int points = BaseKillScore * Combo;
        Score += points;

        OnScoreChanged?.Invoke(Score, Combo);

        Color textColor = Combo > 1 ? new Color(1f, 0.75f, 0.2f) : Color.white;
        HUDController.Instance?.ShowFloatingText(position, $"+{points}", textColor, Combo > 1 ? 32f : 24f);

        if (Combo > 0 && Combo % ComboMilestoneStep == 0)
        {
            OnComboMilestone?.Invoke(position);
        }
    }

    public void TriggerGameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        OnGameOver?.Invoke();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
