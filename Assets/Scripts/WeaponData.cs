using System.Collections.Generic;
using UnityEngine;

public enum WeaponKind
{
    Pistol,
    Shotgun,
    Flamethrower,
    RocketLauncher,
    Sniper
}

// Estadísticas de cada arma. Es una clase de datos simple (no MonoBehaviour) para no
// depender de ScriptableObjects creados a mano en el editor.
public class WeaponStats
{
    public WeaponKind Kind;
    public string DisplayName;
    public float FireRate;      // segundos entre disparos (o entre "ticks" si IsContinuous)
    public int Damage;
    public float ProjectileSpeed;
    public int PelletCount = 1;
    public float SpreadDegrees;
    public Color Color = Color.white;
    public bool IsContinuous;   // lanzallamas: daño por cono en vez de proyectiles
    public float Range = 3f;    // alcance del cono, solo para armas continuas
    public bool IsExplosive;
    public float ExplosionRadius;
    public bool InfiniteAmmo;
    public int MaxAmmo;

    // Caída de daño por distancia (por ahora solo la escopeta la usa: de cerca pega el daño
    // completo por cada perdigón, y de lejos baja hasta MinDamageMultiplier).
    public bool HasFalloff;
    public float FalloffStartRange;
    public float FalloffEndRange;
    public float MinDamageMultiplier = 1f;
}

// Colores bien separados entre sí (verde/naranja/rojo/violeta/celeste) para que cada arma se
// distinga de un vistazo, tanto en el HUD como en los recogibles tirados en el mapa.
public static class WeaponDatabase
{
    public static readonly Dictionary<WeaponKind, WeaponStats> All = new Dictionary<WeaponKind, WeaponStats>
    {
        { WeaponKind.Pistol, new WeaponStats
        {
            Kind = WeaponKind.Pistol, DisplayName = "Pistola", FireRate = 0.3f, Damage = 10,
            ProjectileSpeed = 12f, PelletCount = 1, SpreadDegrees = 0f,
            Color = new Color(0.3f, 1f, 0.4f), InfiniteAmmo = true
        }},
        { WeaponKind.Shotgun, new WeaponStats
        {
            Kind = WeaponKind.Shotgun, DisplayName = "Escopeta", FireRate = 0.65f, Damage = 7,
            ProjectileSpeed = 14f, PelletCount = 6, SpreadDegrees = 32f,
            Color = new Color(1f, 0.6f, 0.05f), InfiniteAmmo = false, MaxAmmo = 18,
            HasFalloff = true, FalloffStartRange = 1.5f, FalloffEndRange = 8f, MinDamageMultiplier = 0.35f
        }},
        { WeaponKind.Flamethrower, new WeaponStats
        {
            Kind = WeaponKind.Flamethrower, DisplayName = "Lanzallamas", FireRate = 0.08f, Damage = 4,
            IsContinuous = true, Range = 3.2f, SpreadDegrees = 26f,
            Color = new Color(1f, 0.15f, 0.1f), InfiniteAmmo = false, MaxAmmo = 140
        }},
        { WeaponKind.Sniper, new WeaponStats
        {
            Kind = WeaponKind.Sniper, DisplayName = "Rifle de Francotirador", FireRate = 1.1f, Damage = 45,
            ProjectileSpeed = 22f, PelletCount = 1, SpreadDegrees = 0f,
            Color = new Color(0.2f, 0.6f, 1f), InfiniteAmmo = false, MaxAmmo = 10
        }},
        { WeaponKind.RocketLauncher, new WeaponStats
        {
            Kind = WeaponKind.RocketLauncher, DisplayName = "Lanzacohetes", FireRate = 0.9f, Damage = 35,
            ProjectileSpeed = 9f, PelletCount = 1, SpreadDegrees = 0f, IsExplosive = true, ExplosionRadius = 2.2f,
            Color = new Color(0.8f, 0.2f, 1f), InfiniteAmmo = false, MaxAmmo = 6
        }},
    };
}
