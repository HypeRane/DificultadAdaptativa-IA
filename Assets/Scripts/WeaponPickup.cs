using UnityEngine;

// Recogible de arma: bobbing + rotación suave, y al tocar al jugador le da munición de esa arma
// y la equipa automáticamente.
public class WeaponPickup : MonoBehaviour
{
    private WeaponKind kind;
    private int ammoAmount;
    private float bobTimer;
    private float baseY;

    public void Init(WeaponKind weaponKind, int amount)
    {
        kind = weaponKind;
        ammoAmount = amount;
        baseY = transform.position.y;

        WeaponStats stats = WeaponDatabase.All[kind];

        transform.localScale = Vector3.one * 0.6f;

        SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = ProceduralSprites.Circle;
        sr.sharedMaterial = ProceduralSprites.WorldSpriteMaterial;
        sr.color = stats.Color;
        sr.sortingOrder = 3;

        GameObject glow = new GameObject("Glow");
        glow.transform.SetParent(transform, false);
        glow.transform.localScale = Vector3.one * 2.4f;
        SpriteRenderer glowSr = glow.AddComponent<SpriteRenderer>();
        glowSr.sprite = ProceduralSprites.SoftGlow;
        glowSr.sharedMaterial = ProceduralSprites.WorldSpriteMaterial;
        glowSr.color = new Color(stats.Color.r, stats.Color.g, stats.Color.b, 0.35f);
        glowSr.sortingOrder = -1;

        CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.6f;
    }

    private void Update()
    {
        bobTimer += Time.deltaTime;
        Vector3 pos = transform.position;
        pos.y = baseY + Mathf.Sin(bobTimer * 3f) * 0.15f;
        transform.position = pos;
        transform.Rotate(0f, 0f, 60f * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerShooting shooting = other.GetComponent<PlayerShooting>();
        WeaponStats stats = WeaponDatabase.All[kind];

        if (shooting != null)
        {
            shooting.AddAmmo(kind, ammoAmount);
            HUDController.Instance?.ShowFloatingText(transform.position, stats.DisplayName, stats.Color, 24f);
        }

        HitEffects.SpawnBurst(transform.position, stats.Color, 8, 3f, 0.3f);
        Destroy(gameObject);
    }
}
