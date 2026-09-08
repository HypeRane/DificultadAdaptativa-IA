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

        int reduced = Mathf.Max(1, Mathf.RoundToInt(amount * PerkEffects.DamageTakenMultiplier));

        DifficultyManager.Instance?.RegisterPlayerDamaged(reduced);
        CurrentHealth = Mathf.Max(0, CurrentHealth - reduced);

        OnDamaged?.Invoke(reduced);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        CameraFollow.Shake(0.15f, 0.12f);
        SoundManager.Play(Sfx.PlayerHurt);

        Debug.Log($"Jugador recibió {reduced} de daño. Vida actual: {CurrentHealth}");

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    // Usado por perks (Botiquín) y por el vampirismo (curación por kill).
    public void Heal(int amount)
    {
        if (isDead || amount <= 0) return;

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    // Usado por el perk Vitalidad: sube el tope de vida y cura esa misma cantidad al instante.
    public void IncreaseMaxHealth(int amount)
    {
        if (amount <= 0) return;

        maxHealth += amount;
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
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
