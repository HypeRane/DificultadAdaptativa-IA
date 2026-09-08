using UnityEngine;

// Utilidad de spawn para el shmup vertical: un punto aleatorio arriba del borde superior de la
// cámara (que hace scroll continuo), para que enemigos, el jefe y los recogibles entren en
// pantalla desde arriba en vez de aparecer de la nada frente al jugador.
public static class MapUtility
{
    public static Vector2 RandomPointAboveCamera(float margin = 2f, float widthFraction = 0.85f)
    {
        Camera cam = Camera.main;
        float halfHeight = cam != null ? cam.orthographicSize : 7.5f;
        float halfWidth = cam != null ? halfHeight * cam.aspect : 12f;
        Vector3 camPos = cam != null ? cam.transform.position : Vector3.zero;

        for (int attempt = 0; attempt < 10; attempt++)
        {
            float x = Random.Range(camPos.x - halfWidth * widthFraction, camPos.x + halfWidth * widthFraction);
            float y = camPos.y + halfHeight + margin;
            Vector2 candidate = new Vector2(x, y);
            if (!IsInsideObstacle(candidate)) return candidate;
        }

        return new Vector2(camPos.x, camPos.y + halfHeight + margin);
    }

    private static bool IsInsideObstacle(Vector2 point)
    {
        Collider2D hit = Physics2D.OverlapPoint(point);
        return hit != null && hit.GetComponent<ObstacleMarker>() != null;
    }
}
