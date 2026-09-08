using System.Collections.Generic;
using UnityEngine;

// Hace aparecer recogibles de armas especiales por todo el mapa: cada cierto tiempo al azar,
// como premio por una racha de combo alta, y con más variedad a medida que sube el nivel de
// dificultad de la IA (el lanzacohetes recién empieza a aparecer más adelante).
public class WeaponPickupSpawner : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 14f;
    [SerializeField] private float initialDelay = 3f; // para que el jugador vea un arma apenas empieza
    [SerializeField] private float minDistanceFromCenter = 2.5f;

    private float timer;
    private bool firstSpawnDone;

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
        float threshold = firstSpawnDone ? spawnInterval : initialDelay;
        if (timer >= threshold)
        {
            timer = 0f;
            firstSpawnDone = true;
            SpawnPickup(MapUtility.RandomPointInPlayArea(minDistanceFromCenter));
        }
    }

    private void HandleComboMilestone(Vector3 position)
    {
        SpawnPickup((Vector2)position + Random.insideUnitCircle * 1.8f);
    }

    private void SpawnPickup(Vector2 pos)
    {
        WeaponKind kind = PickWeaponKind();
        WeaponStats stats = WeaponDatabase.All[kind];
        int amount = Mathf.Max(1, Mathf.RoundToInt(stats.MaxAmmo / 2f * PerkEffects.AmmoPickupMultiplier));

        GameObject pickupObj = new GameObject($"Pickup_{kind}");
        pickupObj.transform.position = pos;
        pickupObj.AddComponent<WeaponPickup>().Init(kind, amount);
    }

    private WeaponKind PickWeaponKind()
    {
        int level = DifficultyManager.Instance != null ? DifficultyManager.Instance.DifficultyLevel : 1;

        List<WeaponKind> pool = new List<WeaponKind> { WeaponKind.Shotgun, WeaponKind.Shotgun };
        if (level >= 3) pool.Add(WeaponKind.Flamethrower);
        if (level >= 5) pool.Add(WeaponKind.RocketLauncher);

        return pool[Random.Range(0, pool.Count)];
    }
}
