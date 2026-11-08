using System.Collections;
using UnityEngine;
using TMPro;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    public enum PowerUpType { Invincible, Speed, ScatterShot }

    [Header("Active States")]
    public bool isInvincible = false;
    public bool isSpeedBoosted = false;
    public bool isScatterActive = false;

    [Header("Settings & Timers")]
    [SerializeField] private float powerUpDuration = 7f;
    [SerializeField] private float flickerInterval = 0.15f;

    [Header("Screen HUD Connections")]
    [Tooltip("Drag your Screen Canvas text field here.")]
    [SerializeField] private TMP_Text screenPowerUpText;

    [Header("Floating Overhead HUD Connections")]
    [Tooltip("Drag the small floating text object located above the player ship here.")]
    [SerializeField] private TMP_Text floatingCountdownText;

    [Header("Player Mesh References")]
    [Tooltip("Drag the MeshRenderer of your player drone ship model here to enable flickering.")]
    [SerializeField] private MeshRenderer playerMeshRenderer;

    private Coroutine activePowerUpRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ResetHUDText();
    }

    public void HandlePowerUp(PowerUpType type)
    {
        // Cancel any currently running powerup to prevent overlapping timer bugs
        if (activePowerUpRoutine != null)
        {
            StopAllCoroutines();
            ResetStates();
        }

        activePowerUpRoutine = StartCoroutine(ExecutePowerUpLifecycle(type));
    }

    private IEnumerator ExecutePowerUpLifecycle(PowerUpType type)
    {
        string powerUpName = "";

        //Configure the active power-up modifications
        switch (type)
        {
            case PowerUpType.Invincible:
                isInvincible = true;
                powerUpName = "INVINCIBILITY SHIELD";
                StartCoroutine(FlickerMeshRoutine()); 
                break;
            case PowerUpType.Speed:
                isSpeedBoosted = true;
                powerUpName = "THRUSTER BOOST";
                break;
            case PowerUpType.ScatterShot:
                isScatterActive = true;
                powerUpName = "SCATTER-SHOT LASER";
                break;
        }

        // Update UI Display Strings 
        if (screenPowerUpText != null) screenPowerUpText.text = $"POWERUP: {powerUpName}";

        //Countdown Loop (Updates every second)
        float timeRemaining = powerUpDuration;
        while (timeRemaining > 0)
        {
            if (floatingCountdownText != null)
                floatingCountdownText.text = Mathf.CeilToInt(timeRemaining).ToString();

            yield return new WaitForSeconds(1f);
            timeRemaining -= 1f;
        }

        //Clear Buffs upon Expiration
        ResetStates();
        ResetHUDText();
        Debug.Log($"[Powerup Manager] {powerUpName} expired cleanly.");
    }

    private IEnumerator FlickerMeshRoutine()
    {
        while (isInvincible && playerMeshRenderer != null)
        {
            playerMeshRenderer.enabled = !playerMeshRenderer.enabled; // Toggle visibility state
            yield return new WaitForSeconds(flickerInterval);
        }

        if (playerMeshRenderer != null) playerMeshRenderer.enabled = true; // Ensure mesh left fully visible
    }

    private void ResetStates()
    {
        isInvincible = false;
        isSpeedBoosted = false;
        isScatterActive = false;
        if (playerMeshRenderer != null) playerMeshRenderer.enabled = true;
    }

    private void ResetHUDText()
    {
        if (screenPowerUpText != null) screenPowerUpText.text = "POWERUP: NONE";
        if (floatingCountdownText != null) floatingCountdownText.text = "";
    }
}
