using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }

    public event Action<int, int> OnHealthChanged; // current, max
    public event Action<int> OnDamaged; // amount
    public event Action OnDied;

    private bool isDead;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        DifficultyManager.Instance?.RegisterPlayerDamaged(amount);
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);

        OnDamaged?.Invoke(amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        CameraFollow.Shake(0.15f, 0.12f);

        Debug.Log($"Jugador recibió {amount} de daño. Vida actual: {CurrentHealth}");

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Jugador murió");
        OnDied?.Invoke();
        GameManager.Instance?.TriggerGameOver();
    }
}
