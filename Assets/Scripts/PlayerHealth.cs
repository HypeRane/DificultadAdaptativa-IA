using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;

    public int CurrentHealth { get; private set; }

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        DifficultyManager.Instance?.RegisterPlayerDamaged(amount);
        CurrentHealth -= amount;
        Debug.Log($"Jugador recibió {amount} de daño. Vida actual: {CurrentHealth}");

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Jugador murió");
        // Acá más adelante reiniciamos la oleada o mostramos game over
    }
}
