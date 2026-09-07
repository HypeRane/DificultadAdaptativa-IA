using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target; // arrastra el Player acá
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f); // -10 en Z para que la cámara 2D quede detrás
    [SerializeField] private float orthographicSize = 7.5f; // más grande = menos zoom, se ve más mapa

    private static CameraFollow instance;

    private float shakeDuration;
    private float shakeTimeRemaining;
    private float shakeMagnitude;

    private void Awake()
    {
        instance = this;

        Camera cam = GetComponent<Camera>();
        if (cam != null) cam.orthographicSize = orthographicSize;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        if (shakeTimeRemaining > 0f)
        {
            shakeTimeRemaining -= Time.deltaTime;
            float damper = Mathf.Clamp01(shakeTimeRemaining / shakeDuration);
            Vector2 shakeOffset = Random.insideUnitCircle * shakeMagnitude * damper;
            transform.position += new Vector3(shakeOffset.x, shakeOffset.y, 0f);
        }
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
