using UnityEngine;
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
}
