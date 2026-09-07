using UnityEngine;

// Genera sprites y reutiliza el material de sprites del proyecto en tiempo de ejecución,
// para que todos los efectos visuales nuevos no dependan de assets de arte adicionales.
public static class ProceduralSprites
{
    private static Material worldSpriteMaterial;
    private static Sprite square;
    private static Sprite circle;
    private static Sprite ring;
    private static Sprite softGlow;
    private static Sprite vignette;
    private static Sprite roundedRect;

    public static Material WorldSpriteMaterial
    {
        get
        {
            if (worldSpriteMaterial == null)
            {
                SpriteRenderer source = Object.FindAnyObjectByType<SpriteRenderer>();
                if (source != null) worldSpriteMaterial = source.sharedMaterial;
            }
            return worldSpriteMaterial;
        }
    }

    public static Sprite Square => square != null ? square : (square = BuildSprite(8, 8, (u, v) => 1f, 32f));

    public static Sprite Circle => circle != null ? circle : (circle = BuildSprite(64, 64, (u, v) =>
    {
        float d = Distance(u, v) * 2f;
        return Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(0.85f, 1f, d));
    }, 64f));

    public static Sprite Ring => ring != null ? ring : (ring = BuildSprite(64, 64, (u, v) =>
    {
        float d = Distance(u, v) * 2f;
        float outer = Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(0.82f, 1f, d));
        float inner = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.55f, 0.68f, d));
        return outer * inner;
    }, 64f));

    public static Sprite SoftGlow => softGlow != null ? softGlow : (softGlow = BuildSprite(64, 64, (u, v) =>
        Mathf.Clamp01(1f - Distance(u, v) * 2f), 64f));

    public static Sprite Vignette => vignette != null ? vignette : (vignette = BuildSprite(128, 128, (u, v) =>
        Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.5f, 1.3f, Distance(u, v) * 2f)), 128f));

    public static Sprite RoundedRect(int size = 64, float cornerFraction = 0.28f)
    {
        if (roundedRect != null) return roundedRect;
        float radius = size * cornerFraction;
        roundedRect = BuildSprite(size, size, (u, v) =>
        {
            Vector2 p = new Vector2(u * size, v * size);
            Vector2 min = new Vector2(radius, radius);
            Vector2 max = new Vector2(size - radius, size - radius);
            Vector2 clamped = new Vector2(Mathf.Clamp(p.x, min.x, max.x), Mathf.Clamp(p.y, min.y, max.y));
            float dist = Vector2.Distance(p, clamped);
            return Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(radius - 1.5f, radius, dist));
        }, size, new Vector4(radius, radius, radius, radius));
        return roundedRect;
    }

    private static float Distance(float u, float v) => Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f));

    private static Sprite BuildSprite(int width, int height, System.Func<float, float, float> alphaAt, float pixelsPerUnit, Vector4? border = null)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        Color32[] pixels = new Color32[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float u = (x + 0.5f) / width;
                float v = (y + 0.5f) / height;
                float a = Mathf.Clamp01(alphaAt(u, v));
                pixels[y * width + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
        }
        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), pixelsPerUnit, 0, SpriteMeshType.FullRect, border ?? Vector4.zero);
    }
}
