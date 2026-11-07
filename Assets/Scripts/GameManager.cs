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

    [Header("Power-Up UI Overlays")]
    [SerializeField] private TextMeshProUGUI screenPowerUpText;
    [SerializeField] private TextMeshProUGUI floatingCountdownText;

    [Header("Scene Navigation Names")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string gameplayScene = "Gameplay";

    [Header("Linked Reference Components")]
    [SerializeField] private AsteroidSpawner asteroidSpawnerComponent;
    [SerializeField] private EnemyAI enemyAIComponent;
    [SerializeField] private GameObject enemyPrefab;

    private int currentScore = 0;
    private int persistentHighScore = 0;
    private bool isGameActive = true;
    private bool isPaused = false;
    private bool hasDifficultyScaledThisRun = false;
    private Coroutine activeUICountdownRoutine;

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
        persistentHighScore = SaveManager.LoadHighScore();

        InitializeUI();
        UpdateScoreUI();
        ClearPowerUpUI();
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
            SaveManager.SaveHighScore(persistentHighScore);
        }

        UpdateScoreUI();

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

    private void TriggerSuccessDifficultyEscalation()
    {
        Debug.Log("[Game Loop] Player is succeeding! Escalating arena difficulty variables.");

        if (difficultyNotificationText != null)
        {
            difficultyNotificationText.text = "DIFFICULTY INCREASED:\nMore Asteroids & Faster Enemies Incoming!";
            Invoke(nameof(ClearDifficultyNotification), 4.5f);
        }

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
        }
    }

    public void StartPowerUpUITimer(string powerUpName, float duration)
    {
        if (activeUICountdownRoutine != null)
        {
            StopCoroutine(activeUICountdownRoutine);
        }
        activeUICountdownRoutine = StartCoroutine(PowerUpUICountdownRoutine(powerUpName, duration));
    }

    private System.Collections.IEnumerator PowerUpUICountdownRoutine(string powerUpName, float duration)
    {
        if (screenPowerUpText != null) screenPowerUpText.text = $"POWERUP: {powerUpName}";

        float timeRemaining = duration;
        while (timeRemaining > 0)
        {
            if (floatingCountdownText != null) floatingCountdownText.text = Mathf.CeilToInt(timeRemaining).ToString();
            yield return new WaitForSeconds(1f);
            timeRemaining -= 1f;
        }
        ClearPowerUpUI();
    }

    public void ClearPowerUpUI()
    {
        if (screenPowerUpText != null) screenPowerUpText.text = "POWERUP: NONE";
        if (floatingCountdownText != null) floatingCountdownText.text = "";
    }

    public void ResetHighScoreButtonCommand()
    {
        SaveManager.ResetPersistentData();
        persistentHighScore = 0;
        UpdateScoreUI();
    }

    private void ClearDifficultyNotification()
    {
        if (difficultyNotificationText != null)
        {
            difficultyNotificationText.text = "Threat Level: Maximum. Survival mode engaged.";
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = "Scrap Collected: " + currentScore + " / " + targetWinScore;
        if (highScoreText != null) highScoreText.text = "High Score: " + persistentHighScore;
    }

    // --- INTEGRATED COLLECTIBLE MANAGER COMPATIBILITY METHOD ---
    public void SetTargetWinScore(int amount)
    {
        targetWinScore = amount;
        UpdateScoreUI();
        Debug.Log($"[GameManager] Target win score dynamically configured to: {targetWinScore}");
    }

    public void TriggerGameOver()
    {
        isGameActive = false;
        Time.timeScale = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void TriggerWinState()
    {
        isGameActive = false;
        Time.timeScale = 0f;
        if (winPanel != null) winPanel.SetActive(true);
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

    public void RestartGame()
    {
        SceneManager.LoadScene(gameplayScene);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
