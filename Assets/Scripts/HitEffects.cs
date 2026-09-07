using UnityEngine;

// Punto de entrada estático para disparar pequeños estallidos de partículas (impacto, muerte, disparo).
public static class HitEffects
{
    public static void SpawnBurst(Vector3 position, Color color, int count = 8, float speed = 4f, float lifetime = 0.4f)
    {
        Material mat = ProceduralSprites.WorldSpriteMaterial;
        Sprite sprite = ProceduralSprites.Square;

        for (int i = 0; i < count; i++)
        {
            GameObject piece = new GameObject("HitDebris");
            piece.transform.position = position;
            piece.transform.localScale = Vector3.one * Random.Range(0.06f, 0.14f);

            SpriteRenderer sr = piece.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            if (mat != null) sr.sharedMaterial = mat;
            sr.color = color;
            sr.sortingOrder = 10;

            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            piece.AddComponent<DebrisFX>().Init(dir * speed * Random.Range(0.5f, 1f), lifetime);
        }
    }
}
