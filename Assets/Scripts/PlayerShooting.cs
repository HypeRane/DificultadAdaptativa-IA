using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Disparo")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint; // punto de donde sale el disparo; si lo dejas vacío, usa la posición del jugador
    [SerializeField] private float fireRate = 0.3f; // segundos entre disparos
    [SerializeField] private float projectileSpeed = 12f;

    private Camera cam;
    private float nextFireTime;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        // Mantén click izquierdo para disparo automático respetando el cooldown
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
         DifficultyManager.Instance?.RegisterShotFired(); // ← nueva línea
        if (projectilePrefab == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector2 direction = GetDirectionToMouse(spawnPos);

        GameObject projectileObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectileObj.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.SetDirection(direction, projectileSpeed);
        }
    }

    private Vector2 GetDirectionToMouse(Vector3 fromPosition)
    {
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        return (mouseWorldPos - fromPosition).normalized;
    }
}
