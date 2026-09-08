using UnityEngine;

// Hace aparecer mascotas recogibles: cada cierto tiempo al azar, y como premio por rachas de
// combo altas (se suscribe a GameManager.OnComboMilestone).
public class PetPickupSpawner : MonoBehaviour
{
    [SerializeField] private float randomInterval = 55f;
    [SerializeField] private int maxConcurrentPickups = 1;

    private float timer;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnComboMilestone += HandleComboMilestone;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnComboMilestone -= HandleComboMilestone;
        }
    }

    private void Update()
    {
        // Mismo criterio de riesgo/recompensa que WeaponPickupSpawner: el dron aparece antes si
        // el jugador la está pasando mal.
        float skill = DifficultyManager.Instance != null ? DifficultyManager.Instance.SkillFactor : 0.5f;
        float intervalScale = Mathf.Lerp(0.65f, 1.1f, skill);

        timer += Time.deltaTime;
        if (timer >= randomInterval * intervalScale)
        {
            timer = 0f;
            SpawnAt(MapUtility.RandomPointAboveCamera());
        }
    }

    private void HandleComboMilestone(Vector3 position)
    {
        SpawnAt(position + (Vector3)(Random.insideUnitCircle * 1.5f));
    }

    private void SpawnAt(Vector2 pos)
    {
        if (Object.FindObjectsByType<PetPickup>(FindObjectsSortMode.None).Length >= maxConcurrentPickups) return;

        GameObject obj = new GameObject("PetPickup");
        obj.transform.position = pos;
        obj.AddComponent<PetPickup>();
    }
}
