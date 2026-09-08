// Multiplicadores globales que los perks (ver PerkDatabase.cs) van modificando en tiempo real.
// Los sistemas relevantes (disparo, movimiento, vida, mascota, puntaje) los leen directamente.
public static class PerkEffects
{
    public static float FireRateMultiplier = 1f;
    public static float DamageMultiplier = 1f;
    public static float MoveSpeedMultiplier = 1f;
    public static float PetDamageMultiplier = 1f;
    public static float LifeStealPerKill = 0f;
    public static float ScoreMultiplier = 1f;
    public static float DamageTakenMultiplier = 1f;
    public static float AmmoPickupMultiplier = 1f;

    // Se llama al arrancar una partida nueva (GameManager.Awake) para que los perks no se
    // arrastren de una run a otra.
    public static void Reset()
    {
        FireRateMultiplier = 1f;
        DamageMultiplier = 1f;
        MoveSpeedMultiplier = 1f;
        PetDamageMultiplier = 1f;
        LifeStealPerKill = 0f;
        ScoreMultiplier = 1f;
        DamageTakenMultiplier = 1f;
        AmmoPickupMultiplier = 1f;
    }
}
