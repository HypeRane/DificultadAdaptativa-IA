using UnityEngine;
using UnityEngine.Rendering.Universal;

// Tiñe la Global Light 2D de la escena según el nivel de dificultad actual: empieza en un
// blanco/celeste calmado y se va tornando más rojizo e intenso a medida que la IA sube la apuesta.
public class DifficultyAmbiance : MonoBehaviour
{
    private static readonly Color CalmColor = new Color(0.85f, 0.92f, 1f);
    private static readonly Color IntenseColor = new Color(1f, 0.35f, 0.3f);

    private Light2D globalLight;

    private void Awake()
    {
        globalLight = FindGlobalLight();
    }

    private void Update()
    {
        if (globalLight == null || DifficultyManager.Instance == null) return;

        float t = Mathf.InverseLerp(1, 10, DifficultyManager.Instance.DifficultyLevel);
        Color target = Color.Lerp(CalmColor, IntenseColor, t);
        globalLight.color = Color.Lerp(globalLight.color, target, Time.deltaTime * 0.5f);
        globalLight.intensity = Mathf.Lerp(globalLight.intensity, Mathf.Lerp(1f, 1.25f, t), Time.deltaTime * 0.5f);
    }

    private Light2D FindGlobalLight()
    {
        Light2D[] lights = Object.FindObjectsByType<Light2D>(FindObjectsSortMode.None);
        foreach (Light2D l in lights)
        {
            if (l.lightType == Light2D.LightType.Global) return l;
        }
        return null;
    }
}
