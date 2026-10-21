using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [System.Serializable]
    public class GameData
    {
        public int highScore;
    }

    private static string saveFilePath;

    private static void InitializePath()
    {
        // Sets the file path location directly to Unity's safe Persistent Data path directory
        if (string.IsNullOrEmpty(saveFilePath))
        {
            saveFilePath = Path.Combine(Application.persistentDataPath, "saved_game_metrics.json");
        }
    }

    /// <summary>
    /// Custom method to serialize and write high score data to a JSON file on disk (Section A/B requirement)
    /// </summary>
    public static void SaveHighScore(int scoreToSave)
    {
        InitializePath();
        int currentHighScore = LoadHighScore();

        // Only overwrite if the new performance beats the historic best record milestone
        if (scoreToSave > currentHighScore)
        {
            GameData dataToSave = new GameData();
            dataToSave.highScore = scoreToSave;

            string jsonString = JsonUtility.ToJson(dataToSave, true);

            File.WriteAllText(saveFilePath, jsonString);

            Debug.Log($"[SaveManager] New High Score Saved to JSON: {scoreToSave} scrap! Path: {saveFilePath}");
        }
    }

    /// <summary>
    /// Custom method to read and deserialize game score data from a JSON file (Section A/B requirement)
    /// </summary>
    public static int LoadHighScore()
    {
        InitializePath();

        // Check if a saved file structure exists before attempting parsing execution pipelines
        if (File.Exists(saveFilePath))
        {
            try
            {
                string jsonString = File.ReadAllText(saveFilePath);

                GameData loadedData = JsonUtility.FromJson<GameData>(jsonString);

                return loadedData.highScore;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to read JSON payload data file configurations: {e.Message}");
                return 0;
            }
        }

        // Return a baseline value of zero if no persistent record footprint exists yet
        return 0;
    }

    /// <summary>
    /// Deletes the persistent JSON score file from system disk storage.
    /// </summary>
    public static void ResetPersistentData()
    {
        InitializePath();

        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("[SaveManager] Persistent JSON data file wiped from hard drive successfully.");
        }
    }
}
