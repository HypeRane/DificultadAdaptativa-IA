using UnityEngine;

// Shmup vertical: la cámara ya no sigue al jugador, hace scroll automático y continuo hacia
// arriba a X fija. El jugador se mueve libre dentro de un área acotada relativa a la cámara
// (ver PlayerMovement.ClampToScreen).
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 2f; // unidades de mundo por segundo
    [SerializeField] private float orthographicSize = 7.5f;

    private static CameraFollow instance;

    private float shakeDuration;
    private float shakeTimeRemaining;
    private float shakeMagnitude;

    private void Awake()
    {
        instance = this;

        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.orthographicSize = orthographicSize;
            cam.backgroundColor = new Color(0.01f, 0.02f, 0.015f, 1f);
        }
    }

    private void LateUpdate()
    {
        Vector3 pos = transform.position;
        pos.x = 0f;
        pos.y += scrollSpeed * Time.deltaTime;

        if (shakeTimeRemaining > 0f)
        {
            shakeTimeRemaining -= Time.deltaTime;
            float damper = Mathf.Clamp01(shakeTimeRemaining / shakeDuration);
            Vector2 shakeOffset = Random.insideUnitCircle * shakeMagnitude * damper;
            pos += new Vector3(shakeOffset.x, shakeOffset.y, 0f);
        }

        transform.position = pos;
    }

    // Sacude la cámara brevemente (golpes recibidos, enemigos muertos, etc).
    public static void Shake(float duration, float magnitude)
    {
        if (instance == null) return;
        instance.shakeDuration = duration;
        instance.shakeTimeRemaining = duration;
        instance.shakeMagnitude = magnitude;
    }
}
