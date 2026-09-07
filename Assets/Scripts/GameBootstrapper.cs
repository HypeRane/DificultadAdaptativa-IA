using UnityEngine;

// Crea en tiempo de ejecución todo lo que el juego necesita (HUD, GameManager, fondo) sin tocar la escena a mano.
// Así funciona apenas se presiona Play, sin importar qué objetos haya en SampleScene.
public static class GameBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (GameManager.Instance == null)
        {
            new GameObject("GameManager").AddComponent<GameManager>();
        }

        if (Object.FindAnyObjectByType<ArenaBackground>() == null)
        {
            new GameObject("ArenaBackground").AddComponent<ArenaBackground>();
        }

        if (Object.FindAnyObjectByType<ObstacleField>() == null)
        {
            new GameObject("ObstacleField").AddComponent<ObstacleField>();
        }

        if (Object.FindAnyObjectByType<WeaponPickupSpawner>() == null)
        {
            new GameObject("WeaponPickupSpawner").AddComponent<WeaponPickupSpawner>();
        }

        if (Object.FindAnyObjectByType<PetPickupSpawner>() == null)
        {
            new GameObject("PetPickupSpawner").AddComponent<PetPickupSpawner>();
        }

        if (HUDController.Instance == null)
        {
            new GameObject("HUD").AddComponent<HUDController>();
        }
    }
}
