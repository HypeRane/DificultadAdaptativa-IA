using System.Collections.Generic;
using UnityEngine;

// Genera obstáculos estáticos una sola vez al iniciar la partida para darle forma al mapa
// (cobertura, cuellos de botella). Son sólidos: bloquean a jugador y enemigos por igual.
public class ObstacleField : MonoBehaviour
{
    [SerializeField] private int obstacleCount = 18;
    [SerializeField] private float minDistanceFromCenter = 2.5f;
    [SerializeField] private float minSpacing = 2.2f;

    private void Awake()
    {
        List<Vector2> placed = new List<Vector2>();
        int attempts = 0;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Vector2? playerPos = playerObj != null ? (Vector2?)playerObj.transform.position : null;

        while (placed.Count < obstacleCount && attempts < obstacleCount * 20)
        {
            attempts++;
            Vector2 candidate = MapUtility.RandomPointInPlayArea(minDistanceFromCenter);

            if (playerPos.HasValue && Vector2.Distance(candidate, playerPos.Value) < 2f) continue;

            bool tooClose = false;
            foreach (Vector2 p in placed)
            {
                if (Vector2.Distance(p, candidate) < minSpacing) { tooClose = true; break; }
            }
            if (tooClose) continue;

            placed.Add(candidate);
            SpawnObstacle(candidate);
        }
    }

    private void SpawnObstacle(Vector2 pos)
    {
        bool round = Random.value > 0.5f;
        float size = Random.Range(0.9f, 1.6f);

        GameObject obj = new GameObject("Obstacle");
        obj.transform.position = pos;
        obj.transform.localScale = Vector3.one * size;
        obj.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = round ? ProceduralSprites.Circle : ProceduralSprites.RoundedRect();
        sr.sharedMaterial = ProceduralSprites.WorldSpriteMaterial;
        sr.color = new Color(0.32f, 0.28f, 0.24f);
        sr.sortingOrder = 1;

        if (round)
        {
            CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
            col.radius = 0.42f;
        }
        else
        {
            BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.85f, 0.85f);
        }
    }
}
