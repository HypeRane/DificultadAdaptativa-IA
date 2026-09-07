using UnityEngine;

// Fragmento individual de un estallido de impacto/muerte. Se anima y se autodestruye solo.
public class DebrisFX : MonoBehaviour
{
    private Vector2 velocity;
    private float lifetime;
    private float age;
    private SpriteRenderer sr;
    private Color startColor;

    public void Init(Vector2 initialVelocity, float life)
    {
        velocity = initialVelocity;
        lifetime = life;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) startColor = sr.color;
    }

    private void Update()
    {
        age += Time.deltaTime;
        transform.position += (Vector3)(velocity * Time.deltaTime);
        velocity *= 0.9f;

        float t = age / lifetime;
        if (sr != null)
        {
            Color c = startColor;
            c.a = startColor.a * (1f - t);
            sr.color = c;
        }
        transform.localScale *= Mathf.Clamp01(1f - Time.deltaTime * 1.5f);

        if (age >= lifetime) Destroy(gameObject);
    }
}
