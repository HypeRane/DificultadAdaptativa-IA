using System.Collections.Generic;
using UnityEngine;

// Ofrece 3 perks a elegir (roguelite) cada vez que la IA de dificultad sube a un nivel par,
// y también al derrotar un jefe. Pausa el juego mientras se elige (ver HUDController.ShowPerkChoice).
public class PerkManager : MonoBehaviour
{
    public static PerkManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.OnDifficultyChanged += HandleDifficultyChanged;
        }
        BossController.OnAnyBossDefeated += HandleBossDefeated;
    }

    private void OnDestroy()
    {
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.OnDifficultyChanged -= HandleDifficultyChanged;
        }
        BossController.OnAnyBossDefeated -= HandleBossDefeated;
    }

    private void HandleDifficultyChanged(bool increased)
    {
        if (!increased || DifficultyManager.Instance == null) return;
        if (DifficultyManager.Instance.DifficultyLevel % 2 == 0) OfferPerk();
    }

    private void HandleBossDefeated()
    {
        OfferPerk();
    }

    private void OfferPerk()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        List<PerkDefinition> pool = new List<PerkDefinition>(PerkDatabase.All);
        List<PerkDefinition> choices = new List<PerkDefinition>();
        for (int i = 0; i < 3 && pool.Count > 0; i++)
        {
            int idx = Random.Range(0, pool.Count);
            choices.Add(pool[idx]);
            pool.RemoveAt(idx);
        }

        HUDController.Instance?.ShowPerkChoice(choices, ApplyPerk);
    }

    private void ApplyPerk(PerkDefinition perk)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) perk.Apply?.Invoke(player);

        SoundManager.Play(Sfx.PerkSelect);
        HUDController.Instance?.ShowFloatingText(player != null ? player.transform.position : Vector3.zero, perk.Name, perk.Color, 26f);
    }
}
