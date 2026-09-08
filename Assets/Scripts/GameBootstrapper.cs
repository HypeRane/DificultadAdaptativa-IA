using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// Crea en tiempo de ejecución todo lo que el juego necesita (HUD, GameManager, fondo, spawners)
// sin tocar la escena a mano. RuntimeInitializeOnLoadMethod solo corre UNA vez por sesión de
// juego (no una vez por escena), así que además nos suscribimos a sceneLoaded para que todo se
// vuelva a construir cada vez que se recarga la escena (por ejemplo, al reiniciar tras morir).
public static class GameBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        BuildEverything();
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BuildEverything();
    }

    private static void BuildEverything()
    {
        if (GameManager.Instance == null)
        {
            new GameObject("GameManager").AddComponent<GameManager>();
        }

        if (SoundManager.Instance == null)
        {
            new GameObject("SoundManager").AddComponent<SoundManager>();
        }

        if (PlayerTelemetryTracker.Instance == null)
        {
            new GameObject("PlayerTelemetryTracker").AddComponent<PlayerTelemetryTracker>();
        }

        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        if (Object.FindAnyObjectByType<ArenaBackground>() == null)
        {
            new GameObject("ArenaBackground").AddComponent<ArenaBackground>();
        }

        if (Object.FindAnyObjectByType<DifficultyAmbiance>() == null)
        {
            new GameObject("DifficultyAmbiance").AddComponent<DifficultyAmbiance>();
        }

        if (Object.FindAnyObjectByType<WeaponPickupSpawner>() == null)
        {
            new GameObject("WeaponPickupSpawner").AddComponent<WeaponPickupSpawner>();
        }

        if (Object.FindAnyObjectByType<PetPickupSpawner>() == null)
        {
            new GameObject("PetPickupSpawner").AddComponent<PetPickupSpawner>();
        }

        if (Object.FindAnyObjectByType<BossDirector>() == null)
        {
            new GameObject("BossDirector").AddComponent<BossDirector>();
        }

        if (PerkManager.Instance == null)
        {
            new GameObject("PerkManager").AddComponent<PerkManager>();
        }

        if (HUDController.Instance == null)
        {
            new GameObject("HUD").AddComponent<HUDController>();
        }
    }
}
