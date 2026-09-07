using UnityEngine;

// Disparo de la mascota. Es su propia clase (en vez de reutilizar Projectile.cs) para que no
// cuente en las métricas de precisión del jugador, que son solo de sus propios disparos.
public class PetProjectile : MonoBehaviour
{
    private static readonly Color ShotColor = new Color(0.85f, 0.4f, 1f);

    private Vector2 direction;
    private float speed;
    private int damage;

    public void Init(Vector2 dir, float spd, int dmg)
    {
        direction = dir;
        speed = spd;
        damage = dmg;

        transform.localScale = Vector3.one * 0.22f;

        SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = ProceduralSprites.Circle;
        sr.sharedMaterial = ProceduralSprites.WorldSpriteMaterial;
        sr.color = ShotColor;
        sr.sortingOrder = 5;

        CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.45f;

        Destroy(gameObject, 3f);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            HitEffects.SpawnBurst(transform.position, ShotColor, 3, 2f, 0.2f);
        }
        Destroy(gameObject);
    }
}
