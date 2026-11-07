using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private const string HighScoreKey = "AsteroidScavenger_HighScore";

    public static void SaveHighScore(int scoreToSave)
    {
        int currentHighScore = LoadHighScore();

        if (scoreToSave > currentHighScore)
        {
            PlayerPrefs.SetInt(HighScoreKey, scoreToSave);
            PlayerPrefs.Save();
            Debug.Log($"[SaveManager] New High Score safely updated in PlayerPrefs: {scoreToSave} scrap!");
        }
    }

    public static int LoadHighScore()
    {
        int loadedScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        Debug.Log($"[SaveManager] Loaded high score from PlayerPrefs: {loadedScore}");
        return loadedScore;
    }

    public static void ResetPersistentData()
    {
        if (PlayerPrefs.HasKey(HighScoreKey))
        {
            PlayerPrefs.DeleteKey(HighScoreKey);
            PlayerPrefs.Save();
            Debug.Log("[SaveManager] Persistent PlayerPrefs high score field wiped successfully.");
        }
    }
}
