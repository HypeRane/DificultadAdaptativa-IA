using UnityEngine;

// Mascota tirada en el suelo: aparece al azar o como premio por una buena racha de combo
// (ver PetPickupSpawner.cs). El jugador la recoge caminando encima.
public class PetPickup : MonoBehaviour
{
    private static readonly Color PetColor = new Color(0.85f, 0.4f, 1f);

    private float bobTimer;
    private float baseY;

    private void Awake()
    {
        baseY = transform.position.y;
        transform.localScale = Vector3.one * 0.55f;

        SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = ProceduralSprites.Circle;
        sr.sharedMaterial = ProceduralSprites.WorldSpriteMaterial;
        sr.color = PetColor;
        sr.sortingOrder = 3;

        GameObject glow = new GameObject("Glow");
        glow.transform.SetParent(transform, false);
        glow.transform.localScale = Vector3.one * 2.6f;
        SpriteRenderer glowSr = glow.AddComponent<SpriteRenderer>();
        glowSr.sprite = ProceduralSprites.SoftGlow;
        glowSr.sharedMaterial = ProceduralSprites.WorldSpriteMaterial;
        glowSr.color = new Color(PetColor.r, PetColor.g, PetColor.b, 0.4f);
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
        transform.Rotate(0f, 0f, 90f * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (PetCompanion.Active != null)
        {
            PetCompanion.Active.RefreshDuration();
            HUDController.Instance?.ShowFloatingText(transform.position, "Mascota renovada", PetColor, 22f);
        }
        else
        {
            new GameObject("PetCompanion").AddComponent<PetCompanion>();
            HUDController.Instance?.ShowFloatingText(transform.position, "¡Mascota!", PetColor, 26f);
        }

        HitEffects.SpawnBurst(transform.position, PetColor, 8, 3f, 0.3f);
        Destroy(gameObject);
    }
}
