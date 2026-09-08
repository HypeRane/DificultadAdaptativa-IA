using System;
using System.Collections.Generic;
using UnityEngine;

// Una mejora roguelite elegible: nombre/descripción para la UI, color, y el efecto que aplica.
public class PerkDefinition
{
    public string Name;
    public string Description;
    public Color Color;
    public Action<GameObject> Apply;
}

public static class PerkDatabase
{
    public static readonly List<PerkDefinition> All = new List<PerkDefinition>
    {
        new PerkDefinition
        {
            Name = "Cadencia rápida", Description = "+20% velocidad de disparo",
            Color = new Color(1f, 0.85f, 0.3f),
            Apply = _ => PerkEffects.FireRateMultiplier *= 0.8f
        },
        new PerkDefinition
        {
            Name = "Más potencia", Description = "+20% de daño en todas las armas",
            Color = new Color(1f, 0.4f, 0.3f),
            Apply = _ => PerkEffects.DamageMultiplier *= 1.2f
        },
        new PerkDefinition
        {
            Name = "Vitalidad", Description = "+20 de vida máxima (cura al instante)",
            Color = new Color(0.4f, 1f, 0.5f),
            Apply = player =>
            {
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                ph?.IncreaseMaxHealth(20);
            }
        },
        new PerkDefinition
        {
            Name = "Botiquín", Description = "Cura el 50% de la vida perdida",
            Color = new Color(0.3f, 1f, 0.7f),
            Apply = player =>
            {
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph != null) ph.Heal(Mathf.RoundToInt((ph.MaxHealth - ph.CurrentHealth) * 0.5f));
            }
        },
        new PerkDefinition
        {
            Name = "Piernas rápidas", Description = "+15% velocidad de movimiento",
            Color = new Color(0.4f, 0.85f, 1f),
            Apply = _ => PerkEffects.MoveSpeedMultiplier *= 1.15f
        },
        new PerkDefinition
        {
            Name = "Dron mejorado", Description = "+35% de daño de tus drones",
            Color = new Color(0.85f, 0.4f, 1f),
            Apply = _ => PerkEffects.PetDamageMultiplier *= 1.35f
        },
        new PerkDefinition
        {
            Name = "Vampirismo", Description = "Recuperás 1 de vida por cada eliminación",
            Color = new Color(0.8f, 0.1f, 0.2f),
            Apply = _ => PerkEffects.LifeStealPerKill += 1f
        },
        new PerkDefinition
        {
            Name = "Cazarrecompensas", Description = "+25% de puntaje por eliminación",
            Color = new Color(1f, 0.9f, 0.2f),
            Apply = _ => PerkEffects.ScoreMultiplier *= 1.25f
        },
        new PerkDefinition
        {
            Name = "Armadura", Description = "-15% de daño recibido",
            Color = new Color(0.6f, 0.6f, 0.65f),
            Apply = _ => PerkEffects.DamageTakenMultiplier *= 0.85f
        },
        new PerkDefinition
        {
            Name = "Bolsillos grandes", Description = "+30% de munición al recoger armas",
            Color = new Color(0.9f, 0.6f, 0.2f),
            Apply = _ => PerkEffects.AmmoPickupMultiplier *= 1.3f
        },
    };
}
