using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnMargin = 2f; // qué tan arriba del borde de cámara aparecen

    private float timer;

    private void Update()
    {
        // SpawnIntervalMultiplier es la capa reactiva del director (Tensión/Valle/Clímax):
        // modula el ritmo en tiempo real por encima de la progresión lenta de SpawnInterval.
        float currentSpawnInterval = DifficultyManager.Instance != null
            ? DifficultyManager.Instance.SpawnInterval * DifficultyManager.Instance.SpawnIntervalMultiplier
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

        Vector2 spawnPos = MapUtility.RandomPointAboveCamera(spawnMargin);
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
        if (level >= 5) pool.Add(EnemyKind.Bomber);
        if (level >= 7) pool.Add(EnemyKind.Elite);

        // En Clímax el director le suma peso extra a los tipos más amenazantes ya desbloqueados,
        // en vez de agregar variedad nueva (eso lo sigue controlando el nivel de dificultad).
        if (DifficultyManager.Instance != null && DifficultyManager.Instance.CurrentState == DirectorState.Climax)
        {
            if (level >= 4) pool.Add(EnemyKind.Tank);
            if (level >= 5) pool.Add(EnemyKind.Bomber);
        }

        return pool[Random.Range(0, pool.Count)];
    }

    // Techo de enemigos vivos a la vez: sin esto, si el intervalo de aparición baja mucho y el
    // jugador no llega a limpiarlos, se acumulan sin control. Crece con el nivel y con el Clímax.
    private int GetConcurrentCap()
    {
        int level = DifficultyManager.Instance != null ? DifficultyManager.Instance.DifficultyLevel : 1;
        int cap = 6 + level;

        if (DifficultyManager.Instance != null && DifficultyManager.Instance.CurrentState == DirectorState.Climax)
        {
            cap += 3;
        }

        return cap;
    }
}
