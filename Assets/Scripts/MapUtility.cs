using UnityEngine;

// Punto aleatorio dentro del área jugable visible (según el tamaño real de la cámara), para que
// obstáculos y recogibles se repartan por todo el mapa y no se amontonen cerca del centro.
public static class MapUtility
{
    public static Vector2 RandomPointInPlayArea(float minDistanceFromCenter = 0f, float boundsMultiplier = 1.15f)
    {
        Camera cam = Camera.main;
        float halfHeight = cam != null ? cam.orthographicSize * boundsMultiplier : 8f;
        float halfWidth = cam != null ? halfHeight * cam.aspect : 12f;

        Vector2 candidate;
        int attempts = 0;
        do
        {
            candidate = new Vector2(Random.Range(-halfWidth, halfWidth), Random.Range(-halfHeight, halfHeight));
            attempts++;
        } while (candidate.magnitude < minDistanceFromCenter && attempts < 12);

        return candidate;
    }
}
