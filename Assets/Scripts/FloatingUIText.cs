using UnityEngine;
using UnityEngine.UI;

// Texto de combate flotante (daño, puntos) que sube y se desvanece solo.
public class FloatingUIText : MonoBehaviour
{
    private RectTransform rt;
    private Text text;
    private float age;
    private readonly float lifetime = 0.8f;
    private readonly Vector2 velocity = new Vector2(0f, 60f);
    private Color startColor;

    public void Init(RectTransform rectTransform, Text textComponent)
    {
        rt = rectTransform;
        text = textComponent;
        startColor = text.color;
    }

    private void Update()
    {
        age += Time.deltaTime;
        rt.anchoredPosition += velocity * Time.deltaTime;

        float t = Mathf.Clamp01(age / lifetime);
        Color c = startColor;
        c.a = startColor.a * (1f - t);
        text.color = c;
        rt.localScale = Vector3.one * Mathf.Lerp(1.15f, 0.9f, t);

        if (age >= lifetime) Destroy(gameObject);
    }
}
