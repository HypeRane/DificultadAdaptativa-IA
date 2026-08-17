using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f; // segundos antes de autodestruirse si no choca con nada
    [SerializeField] private int damage = 10;

    private Vector2 direction;
    private float speed;

    public void SetDirection(Vector2 newDirection, float newSpeed)
    {
        direction = newDirection;
        speed = newSpeed;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                DifficultyManager.Instance?.RegisterShotHit(); // ← nueva línea
            }
            Destroy(gameObject);
        }
    }
}
