using UnityEngine;
using UnityEngine.UI;

// Hace parpadear un texto de UI (usado en el aviso de "presiona R para reintentar").
public class BlinkText : MonoBehaviour
{
    private Text text;

    private void Awake() => text = GetComponent<Text>();

    private void Update()
    {
        if (text == null) return;
        float pulse = (Mathf.Sin(Time.unscaledTime * 4f) + 1f) * 0.5f;
        Color c = text.color;
        c.a = Mathf.Lerp(0.4f, 1f, pulse);
        text.color = c;
    }
}
