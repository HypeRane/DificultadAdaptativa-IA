using UnityEngine;

// Guarda los mejores resultados localmente con PlayerPrefs, para el menú principal y la
// pantalla de game over.
public static class HighScoreManager
{
    private const string BestScoreKey = "AdaptiveDifficulty_BestScore";
    private const string BestComboKey = "AdaptiveDifficulty_BestCombo";
    private const string BestLevelKey = "AdaptiveDifficulty_BestLevel";
    private const string BestSurvivalKey = "AdaptiveDifficulty_BestSurvival";

    public static int BestScore => PlayerPrefs.GetInt(BestScoreKey, 0);
    public static int BestCombo => PlayerPrefs.GetInt(BestComboKey, 0);
    public static int BestLevel => PlayerPrefs.GetInt(BestLevelKey, 1);
    public static float BestSurvivalTime => PlayerPrefs.GetFloat(BestSurvivalKey, 0f);

    // Devuelve true si el puntaje de esta partida superó el récord anterior.
    public static bool SubmitRun(int score, int maxCombo, int difficultyLevel, float survivalTime)
    {
        bool newRecord = score > BestScore;

        if (newRecord) PlayerPrefs.SetInt(BestScoreKey, score);
        if (maxCombo > BestCombo) PlayerPrefs.SetInt(BestComboKey, maxCombo);
        if (difficultyLevel > BestLevel) PlayerPrefs.SetInt(BestLevelKey, difficultyLevel);
        if (survivalTime > BestSurvivalTime) PlayerPrefs.SetFloat(BestSurvivalKey, survivalTime);
        PlayerPrefs.Save();

        return newRecord;
    }
}
