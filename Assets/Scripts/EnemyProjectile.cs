using UnityEngine;

// Proyectil disparado por enemigos "Tirador". Es la contraparte de Projectile.cs pero apuntando
// al jugador; se genera 100% por código, sin necesitar un prefab.
public class EnemyProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private int damage;

    public void Init(Vector2 dir, float spd, int dmg, Color color)
    {
        direction = dir;
        speed = spd;
        damage = dmg;

        transform.localScale = Vector3.one * 0.25f;

        SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = ProceduralSprites.Circle;
        sr.sharedMaterial = ProceduralSprites.WorldSpriteMaterial;
        sr.color = color;
        sr.sortingOrder = 5;

        Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic; // necesario para que detecte triggers contra obstáculos estáticos

        CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        Destroy(gameObject, 4f);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null) playerHealth.TakeDamage(damage);

            HitEffects.SpawnBurst(transform.position, Color.white, 4, 3f, 0.2f);
            Destroy(gameObject);
            return;
        }

        if (other.GetComponent<ObstacleMarker>() != null)
        {
            HitEffects.SpawnBurst(transform.position, new Color(0.5f, 0.45f, 0.4f), 4, 2f, 0.15f);
            Destroy(gameObject);
        }
    }
}
