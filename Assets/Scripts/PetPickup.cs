using UnityEngine;

// Mascota tirada en el suelo: aparece al azar o como premio por una buena racha de combo
// (ver PetPickupSpawner.cs). El jugador la recoge caminando encima.
public class PetPickup : MonoBehaviour
{
    private static readonly Color DroneColor = new Color(0.85f, 0.4f, 1f);

    private float bobTimer;
    private float baseY;

    private void Awake()
    {
        baseY = transform.position.y;
        transform.localScale = Vector3.one * 0.55f;

        SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = ProceduralSprites.Circle;
        sr.sharedMaterial = ProceduralSprites.WorldSpriteMaterial;
        sr.color = DroneColor;
        sr.sortingOrder = 3;

        GameObject glow = new GameObject("Glow");
        glow.transform.SetParent(transform, false);
        glow.transform.localScale = Vector3.one * 2.6f;
        SpriteRenderer glowSr = glow.AddComponent<SpriteRenderer>();
        glowSr.sprite = ProceduralSprites.SoftGlow;
        glowSr.sharedMaterial = ProceduralSprites.WorldSpriteMaterial;
        glowSr.color = new Color(DroneColor.r, DroneColor.g, DroneColor.b, 0.4f);
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

        // Si el scroll de la cámara lo dejó atrás sin que lo agarraras, se limpia — mismo motivo
        // que WeaponPickup: si no, ocuparía el cupo de PetPickupSpawner para siempre.
        Camera cam = Camera.main;
        if (cam != null && transform.position.y < cam.transform.position.y - cam.orthographicSize * 1.5f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (PetCompanion.ActiveCount < PetCompanion.MaxDrones)
        {
            new GameObject("Drone").AddComponent<PetCompanion>();
            HUDController.Instance?.ShowFloatingText(transform.position, "¡Dron desplegado!", DroneColor, 26f);
        }
        else
        {
            PetCompanion.RefreshAll();
            HUDController.Instance?.ShowFloatingText(transform.position, "Drones reactivados", DroneColor, 22f);
        }

        HitEffects.SpawnBurst(transform.position, DroneColor, 8, 3f, 0.3f);
        SoundManager.Play(Sfx.Pickup);
        Destroy(gameObject);
    }
}
