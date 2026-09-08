using System.Collections.Generic;
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
        if (GameObject.FindGameObjectsWithTag("Enemy").Length >= GetConcurrentCap()) return;

        Vector2 spawnPos = GetRandomPointOnCircle(spawnRadius);
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.ApplyArchetype(PickEnemyKind());
            enemy.BeginSpawnAnimation();
        }
    }

    // La variedad de enemigos se va desbloqueando junto con el nivel de dificultad de la IA:
    // al principio solo aparecen Rastreadores, y con el tiempo se suman Corredores, Tiradores y Brutos.
    private EnemyKind PickEnemyKind()
    {
        int level = DifficultyManager.Instance != null ? DifficultyManager.Instance.DifficultyLevel : 1;

        List<EnemyKind> pool = new List<EnemyKind> { EnemyKind.Normal, EnemyKind.Normal, EnemyKind.Normal };
        if (level >= 2) pool.Add(EnemyKind.Fast);
        if (level >= 3) pool.Add(EnemyKind.Ranged);
        if (level >= 4) pool.Add(EnemyKind.Tank);

        return pool[Random.Range(0, pool.Count)];
    }

    // Techo de enemigos vivos a la vez: sin esto, si el intervalo de aparición baja mucho y el
    // jugador no llega a limpiarlos, se acumulan sin control. Crece un poco con el nivel.
    private int GetConcurrentCap()
    {
        int level = DifficultyManager.Instance != null ? DifficultyManager.Instance.DifficultyLevel : 1;
        return 6 + level;
    }

    private Vector2 GetRandomPointOnCircle(float radius)
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
    }
}
