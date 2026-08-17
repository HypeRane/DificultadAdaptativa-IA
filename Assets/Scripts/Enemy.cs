using UnityEngine;

public class Enemy : MonoBehaviour
{
    private float moveSpeed;
    private int maxHealth;
    private int contactDamage;

    private int currentHealth;
    private Transform player;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

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

    private void FixedUpdate()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        DifficultyManager.Instance?.RegisterEnemyKilled();
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage);
            }
        }
    }
}
