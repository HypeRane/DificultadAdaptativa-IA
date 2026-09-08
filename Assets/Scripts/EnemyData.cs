using System.Collections.Generic;
using UnityEngine;

public enum EnemyKind
{
    Normal,
    Fast,
    Tank,
    Ranged,
    Bomber,
    Elite
}

// Multiplicadores sobre las stats base que ya calcula DifficultyManager. Así la IA de dificultad
// sigue controlando la potencia general, y cada arquetipo solo cambia el "sabor" del enemigo.
public class EnemyArchetype
{
    public EnemyKind Kind;
    public string DisplayName;
    public float SpeedMultiplier = 1f;
    public float HealthMultiplier = 1f;
    public float DamageMultiplier = 1f;
    public float ScaleMultiplier = 1f;
    public Color Color = Color.red;

    public bool IsRanged;
    public float PreferredDistance = 4f;
    public float RangedFireInterval = 1.6f;
    public int RangedDamage = 6;
    public float RangedProjectileSpeed = 6f;

    // El Explosivo no ataca por contacto normal: al llegar al jugador (o al morir) detona.
    public bool IsBomber;
    public float BomberExplosionRadius = 2f;
    public int BomberExplosionDamage = 16;
}

// Colores bien separados (rojo/amarillo/violeta/celeste/naranja/rosa) para poder distinguir
// cada tipo de un vistazo mientras juegas.
public static class EnemyDatabase
{
    public static readonly Dictionary<EnemyKind, EnemyArchetype> All = new Dictionary<EnemyKind, EnemyArchetype>
    {
        { EnemyKind.Normal, new EnemyArchetype
        {
            Kind = EnemyKind.Normal, DisplayName = "Devorador",
            SpeedMultiplier = 1f, HealthMultiplier = 1f, DamageMultiplier = 1f, ScaleMultiplier = 1f,
            Color = new Color(0.95f, 0.15f, 0.15f) // rojo
        }},
        { EnemyKind.Fast, new EnemyArchetype
        {
            Kind = EnemyKind.Fast, DisplayName = "Enjambre",
            SpeedMultiplier = 1.9f, HealthMultiplier = 0.45f, DamageMultiplier = 0.7f, ScaleMultiplier = 0.75f,
            Color = new Color(1f, 0.92f, 0.1f) // amarillo
        }},
        { EnemyKind.Tank, new EnemyArchetype
        {
            Kind = EnemyKind.Tank, DisplayName = "Behemoth",
            SpeedMultiplier = 0.55f, HealthMultiplier = 3f, DamageMultiplier = 1.8f, ScaleMultiplier = 1.6f,
            Color = new Color(0.6f, 0.1f, 0.78f) // violeta
        }},
        { EnemyKind.Ranged, new EnemyArchetype
        {
            Kind = EnemyKind.Ranged, DisplayName = "Escupidor",
            SpeedMultiplier = 0.85f, HealthMultiplier = 0.6f, DamageMultiplier = 0.6f, ScaleMultiplier = 0.9f,
            Color = new Color(0.1f, 0.85f, 0.95f), // celeste
            IsRanged = true, PreferredDistance = 4.5f, RangedFireInterval = 2.8f, RangedDamage = 6, RangedProjectileSpeed = 7f
        }},
        { EnemyKind.Bomber, new EnemyArchetype
        {
            Kind = EnemyKind.Bomber, DisplayName = "Explosivo",
            SpeedMultiplier = 2.2f, HealthMultiplier = 0.35f, DamageMultiplier = 0.5f, ScaleMultiplier = 0.85f,
            Color = new Color(1f, 0.5f, 0.05f), // naranja
            IsBomber = true, BomberExplosionRadius = 2.2f, BomberExplosionDamage = 18
        }},
        { EnemyKind.Elite, new EnemyArchetype
        {
            Kind = EnemyKind.Elite, DisplayName = "Élite",
            SpeedMultiplier = 1.3f, HealthMultiplier = 1.8f, DamageMultiplier = 1.5f, ScaleMultiplier = 1.2f,
            Color = new Color(1f, 0.25f, 0.55f) // rosa/magenta
        }},
    };
}
