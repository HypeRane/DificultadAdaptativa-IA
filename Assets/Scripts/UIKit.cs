using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Helpers para construir el HUD por código, sin depender de objetos armados a mano en la escena.
public static class UIKit
{
    private static Font cachedFont;

    public static Font DefaultFont
    {
        get
        {
            if (cachedFont == null)
            {
                cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            if (cachedFont == null)
            {
                cachedFont = Font.CreateDynamicFontFromOSFont("Arial", 16);
            }
            return cachedFont;
        }
    }

    public static RectTransform CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        return rt;
    }

    public static Text CreateText(string name, Transform parent, string content, int size, Color color, TextAnchor anchor)
    {
        RectTransform rt = CreateUIObject(name, parent);
        Text txt = rt.gameObject.AddComponent<Text>();
        txt.font = DefaultFont;
        txt.text = content;
        txt.fontSize = size;
        txt.color = color;
        txt.alignment = anchor;
        txt.fontStyle = FontStyle.Bold;
        txt.horizontalOverflow = HorizontalWrapMode.Wrap;
        txt.verticalOverflow = VerticalWrapMode.Truncate;
        txt.raycastTarget = false;

        Shadow shadow = rt.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
        shadow.effectDistance = new Vector2(1.5f, -1.5f);

        return txt;
    }

    // Botón con fondo redondeado + texto centrado. El tamaño se pasa en sizeDelta; el llamador
    // todavía necesita anclar el RectTransform resultante (igual que con CreateText/CreateUIObject).
    public static Button CreateButton(string name, Transform parent, string label, Color color, Vector2 size, UnityAction onClick)
    {
        RectTransform rt = CreateUIObject(name, parent);
        rt.sizeDelta = size;

        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = ProceduralSprites.RoundedRect();
        img.type = Image.Type.Sliced;
        img.color = color;

        Button btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        ColorBlock colors = btn.colors;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.25f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.2f);
        btn.colors = colors;

        btn.onClick.AddListener(() => SoundManager.Play(Sfx.UIClick));
        if (onClick != null) btn.onClick.AddListener(onClick);

        Text text = CreateText("Label", rt, label, 22, Color.white, TextAnchor.MiddleCenter);
        Anchor(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

        return btn;
    }

    private static void Anchor(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
    }
}
