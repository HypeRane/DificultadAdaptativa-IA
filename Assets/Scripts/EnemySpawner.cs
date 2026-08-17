using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnRadius = 8f;

    private float timer;

    private void Update()
    {
        float currentSpawnInterval = DifficultyManager.Instance != null
            ? DifficultyManager.Instance.SpawnInterval
            : 2f;

        timer += Time.deltaTime;
        if (timer >= currentSpawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        Vector2 spawnPos = GetRandomPointOnCircle(spawnRadius);
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    private Vector2 GetRandomPointOnCircle(float radius)
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
    }
}
