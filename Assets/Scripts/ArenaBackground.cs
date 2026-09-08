using UnityEngine;

// Campo de estrellas para la ambientación espacial: dos capas a distinta densidad/brillo que se
// mueven a distinta fracción de la cámara (paralaje simple), dando sensación de profundidad sin
// necesitar scroll de textura real. Todo generado por código, sin assets de arte.
public class ArenaBackground : MonoBehaviour
{
    private static readonly Color32 SpaceColor = new Color32(2, 3, 12, 255);

    private Transform farLayer;
    private Transform nearLayer;
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;

        farLayer = BuildStarLayer("StarsFar", density: 90, starSize: 1.4f, brightness: 0.55f, tileWorldSize: 10f, z: 8f, sortingOrder: -100).transform;
        nearLayer = BuildStarLayer("StarsNear", density: 35, starSize: 2.6f, brightness: 0.95f, tileWorldSize: 12f, z: 7f, sortingOrder: -99).transform;
    }

    private void LateUpdate()
    {
        if (cam == null) return;

        Vector3 camPos = cam.transform.position;
        farLayer.position = new Vector3(camPos.x * 0.3f, camPos.y * 0.3f, farLayer.position.z);
        nearLayer.position = new Vector3(camPos.x * 0.6f, camPos.y * 0.6f, nearLayer.position.z);
    }

    private GameObject BuildStarLayer(string name, int density, float starSize, float brightness, float tileWorldSize, float z, int sortingOrder)
    {
        GameObject obj = new GameObject(name);
        obj.transform.position = new Vector3(0f, 0f, z);

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = BuildStarTexture(density, starSize, brightness, tileWorldSize);
        sr.sharedMaterial = ProceduralSprites.WorldSpriteMaterial;
        sr.color = Color.white;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(200f, 200f);
        sr.sortingOrder = sortingOrder;

        return obj;
    }

    private Sprite BuildStarTexture(int density, float starSize, float brightness, float tileWorldSize)
    {
        const int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Repeat;

        Color32[] pixels = new Color32[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = SpaceColor;

        for (int i = 0; i < density; i++)
        {
            int cx = Random.Range(0, size);
            int cy = Random.Range(0, size);
            float radius = Random.Range(starSize * 0.5f, starSize);
            float b = brightness * Random.Range(0.6f, 1f);
            byte channel = (byte)Mathf.Clamp(255f * b, 0f, 255f);
            Color32 starColor = new Color32(channel, channel, 255, 255);

            int r = Mathf.CeilToInt(radius);
            for (int dy = -r; dy <= r; dy++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist > radius) continue;

                    int x = (cx + dx + size) % size;
                    int y = (cy + dy + size) % size;
                    float t = 1f - dist / radius;

                    int idx = y * size + x;
                    pixels[idx] = BlendBrighter(pixels[idx], starColor, t);
                }
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();

        // pixelsPerUnit = size/tileWorldSize: así cada mosaico de la textura cubre tileWorldSize unidades de mundo.
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / tileWorldSize);
    }

    private static Color32 BlendBrighter(Color32 a, Color32 b, float t)
    {
        return new Color32(
            (byte)Mathf.Max(a.r, b.r * t),
            (byte)Mathf.Max(a.g, b.g * t),
            (byte)Mathf.Max(a.b, b.b * t),
            255);
    }
}
