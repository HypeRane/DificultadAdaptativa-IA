using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Disparo")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint; // punto de donde sale el disparo; si lo dejas vacío, usa la posición del jugador

    public event Action<WeaponKind, int> OnWeaponChanged; // arma actual, munición restante (-1 = infinita)

    private Camera cam;
    private PlayerMovement playerMovement;
    private WeaponKind currentWeapon = WeaponKind.Pistol;
    private readonly Dictionary<WeaponKind, int> ammo = new Dictionary<WeaponKind, int>();
    private float nextFireTime;
    private float flameTickTimer;

    private void Awake()
    {
        cam = Camera.main;
        playerMovement = GetComponent<PlayerMovement>();

        foreach (WeaponKind kind in Enum.GetValues(typeof(WeaponKind)))
        {
            ammo[kind] = 0;
        }

        UpdateWeaponVisual();
    }

    private void Update()
    {
        HandleSwitchInput();

        WeaponStats stats = WeaponDatabase.All[currentWeapon];

        if (Input.GetMouseButton(0))
        {
            if (stats.IsContinuous)
            {
                FireContinuous(stats);
            }
            else if (Time.time >= nextFireTime)
            {
                FireDiscrete(stats);
                nextFireTime = Time.time + stats.FireRate;
            }
        }
        else
        {
            flameTickTimer = 0f; // así el lanzallamas dispara de inmediato al volver a mantener click
        }
    }

    private void HandleSwitchInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchTo(WeaponKind.Pistol);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchTo(WeaponKind.Shotgun);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchTo(WeaponKind.Flamethrower);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchTo(WeaponKind.RocketLauncher);
    }

    private void FireDiscrete(WeaponStats stats)
    {
        DifficultyManager.Instance?.RegisterShotFired();

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector2 baseDir = GetDirectionToMouse(spawnPos);
        float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;

        int pellets = Mathf.Max(1, stats.PelletCount);
        for (int i = 0; i < pellets; i++)
        {
            float spreadT = pellets > 1 ? (float)i / (pellets - 1) - 0.5f : 0f;
            float angle = baseAngle + spreadT * stats.SpreadDegrees + UnityEngine.Random.Range(-1.5f, 1.5f);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            if (projectilePrefab == null) continue;

            GameObject projectileObj = Instantiate(projectilePrefab, spawnPos, Quaternion.Euler(0f, 0f, angle));
            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.SetDirection(dir, stats.ProjectileSpeed);
                projectile.Configure(stats.Damage, stats.Color, stats.IsExplosive, stats.ExplosionRadius,
                    stats.HasFalloff, stats.FalloffStartRange, stats.FalloffEndRange, stats.MinDamageMultiplier);
            }
        }

        HitEffects.SpawnBurst(spawnPos, stats.Color, pellets > 1 ? 2 : 3, 2f, 0.15f);
        ConsumeAmmo(stats);
    }

    private void FireContinuous(WeaponStats stats)
    {
        flameTickTimer -= Time.deltaTime;
        if (flameTickTimer > 0f) return;
        flameTickTimer = stats.FireRate;

        DifficultyManager.Instance?.RegisterShotFired();

        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        Vector2 baseDir = GetDirectionToMouse(origin);

        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, stats.Range);
        bool hitAny = false;
        foreach (Collider2D col in hits)
        {
            if (!col.CompareTag("Enemy")) continue;

            Vector2 toEnemy = (Vector2)col.transform.position - (Vector2)origin;
            if (Vector2.Angle(baseDir, toEnemy) > stats.SpreadDegrees / 2f) continue;

            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(stats.Damage);
                hitAny = true;
            }
        }

        if (hitAny) DifficultyManager.Instance?.RegisterShotHit();

        HitEffects.SpawnBurst(origin + (Vector3)(baseDir * stats.Range * 0.5f), stats.Color, 3, 1.5f, 0.2f);
        ConsumeAmmo(stats);
    }

    private void ConsumeAmmo(WeaponStats stats)
    {
        if (stats.InfiniteAmmo) return;

        ammo[stats.Kind] = Mathf.Max(0, ammo[stats.Kind] - 1);
        if (ammo[stats.Kind] <= 0)
        {
            SwitchTo(WeaponKind.Pistol); // ya notifica el cambio con el estado final
        }
        else
        {
            OnWeaponChanged?.Invoke(currentWeapon, GetAmmoDisplay(currentWeapon));
        }
    }

    public void SwitchTo(WeaponKind kind)
    {
        if (kind != WeaponKind.Pistol && ammo[kind] <= 0) return;

        currentWeapon = kind;
        flameTickTimer = 0f;
        UpdateWeaponVisual();
        OnWeaponChanged?.Invoke(currentWeapon, GetAmmoDisplay(currentWeapon));
    }

    public void AddAmmo(WeaponKind kind, int amount)
    {
        WeaponStats stats = WeaponDatabase.All[kind];
        ammo[kind] = Mathf.Min(stats.MaxAmmo, ammo[kind] + amount);
        SwitchTo(kind); // recoger un arma la equipa al toque
    }

    private int GetAmmoDisplay(WeaponKind kind)
    {
        WeaponStats stats = WeaponDatabase.All[kind];
        return stats.InfiniteAmmo ? -1 : ammo[kind];
    }

    private void UpdateWeaponVisual()
    {
        if (playerMovement == null || playerMovement.WeaponSpriteRenderer == null) return;

        WeaponStats stats = WeaponDatabase.All[currentWeapon];
        SpriteRenderer sr = playerMovement.WeaponSpriteRenderer;
        sr.color = stats.Color;

        switch (currentWeapon)
        {
            case WeaponKind.Pistol:
                sr.transform.localScale = new Vector3(0.9f, 0.16f, 1f);
                break;
            case WeaponKind.Shotgun:
                sr.transform.localScale = new Vector3(1.05f, 0.3f, 1f);
                break;
            case WeaponKind.Flamethrower:
                sr.transform.localScale = new Vector3(0.7f, 0.34f, 1f);
                break;
            case WeaponKind.RocketLauncher:
                sr.transform.localScale = new Vector3(1f, 0.42f, 1f);
                break;
        }
    }

    private Vector2 GetDirectionToMouse(Vector3 fromPosition)
    {
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        return (mouseWorldPos - fromPosition).normalized;
    }
}
