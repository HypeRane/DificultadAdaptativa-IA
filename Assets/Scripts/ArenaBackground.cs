using UnityEngine;

// Fondo de cuadrícula procedural para darle profundidad a la arena, sin usar assets de arte.
public class ArenaBackground : MonoBehaviour
{
    private void Awake()
    {
        transform.position = new Vector3(0f, 0f, 5f); // más lejos de la cámara para quedar siempre detrás
        transform.localScale = Vector3.one;

        SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = BuildGridSprite();
        sr.sharedMaterial = ProceduralSprites.WorldSpriteMaterial;
        sr.color = new Color(0.55f, 0.65f, 0.85f, 1f);
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(100f, 100f);
        sr.sortingOrder = -100;
    }

    private Sprite BuildGridSprite()
    {
        const int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Repeat;
        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool onLine = x == 0 || y == 0;
                pixels[y * size + x] = onLine ? new Color32(255, 255, 255, 45) : new Color32(255, 255, 255, 10);
            }
        }
        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
    }
}
