using System.Collections.Generic;
using UnityEngine;

public enum EnemyKind
{
    Normal,
    Fast,
    Tank,
    Ranged
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
}

public static class EnemyDatabase
{
    public static readonly Dictionary<EnemyKind, EnemyArchetype> All = new Dictionary<EnemyKind, EnemyArchetype>
    {
        { EnemyKind.Normal, new EnemyArchetype
        {
            Kind = EnemyKind.Normal, DisplayName = "Rastreador",
            SpeedMultiplier = 1f, HealthMultiplier = 1f, DamageMultiplier = 1f, ScaleMultiplier = 1f,
            Color = new Color(0.9f, 0.15f, 0.15f)
        }},
        { EnemyKind.Fast, new EnemyArchetype
        {
            Kind = EnemyKind.Fast, DisplayName = "Corredor",
            SpeedMultiplier = 1.9f, HealthMultiplier = 0.45f, DamageMultiplier = 0.7f, ScaleMultiplier = 0.75f,
            Color = new Color(1f, 0.85f, 0.15f)
        }},
        { EnemyKind.Tank, new EnemyArchetype
        {
            Kind = EnemyKind.Tank, DisplayName = "Bruto",
            SpeedMultiplier = 0.55f, HealthMultiplier = 3f, DamageMultiplier = 1.8f, ScaleMultiplier = 1.6f,
            Color = new Color(0.45f, 0.15f, 0.55f)
        }},
        { EnemyKind.Ranged, new EnemyArchetype
        {
            Kind = EnemyKind.Ranged, DisplayName = "Tirador",
            SpeedMultiplier = 0.85f, HealthMultiplier = 0.6f, DamageMultiplier = 0.6f, ScaleMultiplier = 0.9f,
            Color = new Color(0.15f, 0.75f, 0.9f),
            IsRanged = true, PreferredDistance = 4.5f, RangedFireInterval = 2.8f, RangedDamage = 6, RangedProjectileSpeed = 7f
        }},
    };
}
