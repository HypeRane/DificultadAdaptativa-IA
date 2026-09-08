using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Disparo")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint; // punto de donde sale el disparo; si lo dejas vacío, usa la posición del jugador

    public event Action<WeaponKind, int> OnWeaponChanged; // arma actual, munición restante (-1 = infinita)

    private static readonly Vector2 FireDirection = Vector2.up; // shmup vertical: siempre dispara hacia arriba
    private const float FireAngleDegrees = 90f; // ángulo de Vector2.up en grados (para el spread de perdigones)

    private PlayerMovement playerMovement;
    private WeaponKind currentWeapon = WeaponKind.Pistol;
    private readonly Dictionary<WeaponKind, int> ammo = new Dictionary<WeaponKind, int>();
    private float nextFireTime;
    private float flameTickTimer;
    private AudioSource flameLoop;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();

        foreach (WeaponKind kind in Enum.GetValues(typeof(WeaponKind)))
        {
            ammo[kind] = 0;
        }

        flameLoop = gameObject.AddComponent<AudioSource>();
        flameLoop.clip = AudioKit.FlameLoop();
        flameLoop.loop = true;
        flameLoop.playOnAwake = false;
        flameLoop.volume = 0.2f;
        flameLoop.spatialBlend = 0f;

        UpdateWeaponVisual();
    }

    private void Update()
    {
        if (Time.timeScale <= 0f) return; // menú principal, elección de perk, pausa por game over, etc.

        HandleSwitchInput();

        WeaponStats stats = WeaponDatabase.All[currentWeapon];

        if (Input.GetMouseButton(0))
        {
            if (stats.IsContinuous)
            {
                if (!flameLoop.isPlaying) flameLoop.Play();
                FireContinuous(stats);
            }
            else if (Time.time >= nextFireTime)
            {
                FireDiscrete(stats);
                nextFireTime = Time.time + stats.FireRate * PerkEffects.FireRateMultiplier;
            }
        }
        else
        {
            flameTickTimer = 0f; // así el lanzallamas dispara de inmediato al volver a mantener click
            if (flameLoop.isPlaying) flameLoop.Stop();
        }
    }

    private void HandleSwitchInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchTo(WeaponKind.Pistol);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchTo(WeaponKind.Shotgun);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchTo(WeaponKind.Flamethrower);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchTo(WeaponKind.RocketLauncher);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SwitchTo(WeaponKind.Sniper);
    }

    private void FireDiscrete(WeaponStats stats)
    {
        DifficultyManager.Instance?.RegisterShotFired();
        PlayShootSound(stats.Kind);

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        float baseAngle = FireAngleDegrees;

        int damage = Mathf.RoundToInt(stats.Damage * PerkEffects.DamageMultiplier);

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
                projectile.Configure(damage, stats.Color, stats.IsExplosive, stats.ExplosionRadius,
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
        flameTickTimer = stats.FireRate * PerkEffects.FireRateMultiplier;

        DifficultyManager.Instance?.RegisterShotFired();

        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        Vector2 baseDir = FireDirection;
        int damage = Mathf.RoundToInt(stats.Damage * PerkEffects.DamageMultiplier);

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
                enemy.TakeDamage(damage);
                hitAny = true;
            }
        }

        if (hitAny) DifficultyManager.Instance?.RegisterShotHit();

        HitEffects.SpawnBurst(origin + (Vector3)(baseDir * stats.Range * 0.5f), stats.Color, 3, 1.5f, 0.2f);
        ConsumeAmmo(stats);
    }

    private void PlayShootSound(WeaponKind kind)
    {
        switch (kind)
        {
            case WeaponKind.Shotgun:
                SoundManager.Play(Sfx.ShotgunShoot);
                break;
            case WeaponKind.RocketLauncher:
                SoundManager.Play(Sfx.RocketShoot);
                break;
            case WeaponKind.Sniper:
                SoundManager.Play(Sfx.Shoot, 0.9f, 0.6f);
                break;
            default:
                SoundManager.Play(Sfx.Shoot, 0.7f, UnityEngine.Random.Range(0.95f, 1.05f));
                break;
        }
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
        if (kind != WeaponKind.Flamethrower && flameLoop.isPlaying) flameLoop.Stop();
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

        // Ancho/alto invertidos respecto a antes: el indicador ahora apunta fijo hacia arriba
        // (ver PlayerMovement.BuildAimIndicator), ya no rota hacia el mouse.
        switch (currentWeapon)
        {
            case WeaponKind.Pistol:
                sr.transform.localScale = new Vector3(0.16f, 0.9f, 1f);
                break;
            case WeaponKind.Shotgun:
                sr.transform.localScale = new Vector3(0.3f, 1.05f, 1f);
                break;
            case WeaponKind.Flamethrower:
                sr.transform.localScale = new Vector3(0.34f, 0.7f, 1f);
                break;
            case WeaponKind.RocketLauncher:
                sr.transform.localScale = new Vector3(0.42f, 1f, 1f);
                break;
            case WeaponKind.Sniper:
                sr.transform.localScale = new Vector3(0.12f, 1.3f, 1f);
                break;
        }
    }
}
