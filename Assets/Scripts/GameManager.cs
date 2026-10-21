using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Score & Victory Configuration")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private int targetWinScore = 10;

    [Header("In-Scene UI Overlay Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;

    [Header("Difficulty Scaling HUD Feedback")]
    [SerializeField] private TextMeshProUGUI difficultyNotificationText;

    [Header("Scene Navigation Names")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string gameplayScene = "Gameplay";

    [Header("Linked Reference Components")]
    [SerializeField] private AsteroidsSpawner asteroidSpawnerComponent;
    [SerializeField] private EnemyAI enemyAIComponent;
    [SerializeField] private GameObject enemyPrefab; 

    
    private int currentScore = 0;
    private int persistentHighScore = 0;
    private bool isGameActive = true;
    private bool isPaused = false;

    // Milestone Difficulty Flags
    private bool hasDifficultyScaledThisRun = false;

    // Public property gateway for Enemy AI validation checks
    public bool HasDifficultyScaledThisRun => hasDifficultyScaledThisRun;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("GameManager: Initialized gameplay environment tracking loop.");

        // Load high score from JSON
        persistentHighScore = SaveManager.LoadHighScore();

        InitializeUI();
        UpdateScoreUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && isGameActive)
        {
            TogglePause();
        }

        if (!isGameActive && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    private void InitializeUI()
    {
        Time.timeScale = 1f;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);

        if (difficultyNotificationText != null)
        {
            difficultyNotificationText.text = "Raise your score to increase difficulty!";
        }
    }

    public void AddScore(int points)
    {
        if (!isGameActive) return;

        currentScore += points;

        if (currentScore > persistentHighScore)
        {
            persistentHighScore = currentScore;
        }

        UpdateScoreUI();

        Debug.Log($"[Score Update] Scavenged item processed. Score: {currentScore}/{targetWinScore}");

        // Increase difficulty trigger
        float halfwayThreshold = targetWinScore / 2f;
        if (currentScore >= halfwayThreshold && !hasDifficultyScaledThisRun)
        {
            hasDifficultyScaledThisRun = true;
            TriggerSuccessDifficultyEscalation();
        }

        if (currentScore >= targetWinScore)
        {
            TriggerWinState();
        }
    }

    /// <summary>
    /// Custom dynamic balancing method triggered explicitly by player score milestones.
    /// </summary>
    private void TriggerSuccessDifficultyEscalation()
    {
        Debug.Log("[Game Loop] Player is succeeding! Escallating arena difficulty variables.");

        if (difficultyNotificationText != null)
        {
            difficultyNotificationText.text = "DIFFICULTY INCREASED:\nMore Asteroids & Faster Enemies Incoming!";
            Invoke(nameof(ClearDifficultyNotification), 4.5f);
        }

        // Instantly scan the hierarchy to boost any current enemies that are already alive
        EnemyAI[] allActiveEnemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (EnemyAI enemy in allActiveEnemies)
        {
            if (enemy != null)
            {
                enemy.IncreaseAISpeed(1.3f);
            }
        }

        if (enemyPrefab != null)
        {
            Vector3 reinforcementSpawnPos = new Vector3(Random.Range(-50f, 50f), 0f, 150f);
            GameObject secondaryEnemy = Instantiate(enemyPrefab, reinforcementSpawnPos, Quaternion.identity);

            EnemyAI secondaryAI = secondaryEnemy.GetComponent<EnemyAI>();
            if (secondaryAI != null)
            {
                secondaryAI.IncreaseAISpeed(1.3f);
            }

            Debug.Log("[AI Engine] Milestone cleared! Forced instantaneous spawn of fleet reinforcement drone.");
        }

        if (asteroidSpawnerComponent != null)
        {
            Debug.Log("[Spawner] Environmental density constraints adjusted to yield tighter threat vectors.");
        }
    }

    // Reset highscore method
    /// <summary>
    /// Public UI method to wipe the score records and refresh the HUD text instantly.
    /// </summary>
    public void ResetHighScoreButtonCommand()
    {
        // Wipe the actual file from the computer's storage disk
        SaveManager.ResetPersistentData();

        // Clear out runtime active tracking fields back to baseline defaults
        persistentHighScore = 0;

        // Immediately re-render screen UI texts so it falls back to 0 instantly
        UpdateScoreUI();

        Debug.Log("[Game Loop] High Score counter reset event cleared through UI button click interface.");
    }

    private void ClearDifficultyNotification()
    {
        if (difficultyNotificationText != null)
        {
            difficultyNotificationText.text = "Threat Level: Maximum. Survival mode engaged.";
        }
    }

    public void SetTargetWinScore(int amount)
    {
        targetWinScore = amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Scrap Collected: " + currentScore + " / " + targetWinScore;
        }

        if (highScoreText != null)
        {
            highScoreText.text = "High Score: " + persistentHighScore;
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            if (pausePanel != null) pausePanel.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false);
        }
    }

    public void TriggerGameOver()
    {
        if (!isGameActive) return;

        isGameActive = false;
        Time.timeScale = 0f;

        SaveManager.SaveHighScore(currentScore);

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    private void TriggerWinState()
    {
        if (!isGameActive) return;

        isGameActive = false;
        Time.timeScale = 0f;

        SaveManager.SaveHighScore(currentScore);

        if (winPanel != null) winPanel.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplayScene);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplayScene);
    }
}
