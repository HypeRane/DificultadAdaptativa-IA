using UnityEngine;

// Hace aparecer mascotas recogibles: cada cierto tiempo al azar, y como premio por rachas de
// combo altas (se suscribe a GameManager.OnComboMilestone).
public class PetPickupSpawner : MonoBehaviour
{
    [SerializeField] private float randomInterval = 45f;
    [SerializeField] private float minDistanceFromCenter = 2.5f;

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
        timer += Time.deltaTime;
        if (timer >= randomInterval)
        {
            timer = 0f;
            SpawnAt(MapUtility.RandomPointInPlayArea(minDistanceFromCenter));
        }
    }

    private void HandleComboMilestone(Vector3 position)
    {
        SpawnAt(position + (Vector3)(Random.insideUnitCircle * 1.5f));
    }

    private void SpawnAt(Vector2 pos)
    {
        GameObject obj = new GameObject("PetPickup");
        obj.transform.position = pos;
        obj.AddComponent<PetPickup>();
    }
}
