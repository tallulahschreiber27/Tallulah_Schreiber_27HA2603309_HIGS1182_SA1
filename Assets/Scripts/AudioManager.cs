using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Static instance allows other scripts to access this easily
    public static AudioManager Instance { get; private set; }

    [Header("--- Audio Sources ---")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("--- Audio Clips ---")]
    public AudioClip backgroundMusic;
    public AudioClip laserFire;
    public AudioClip asteroidDestroyed;
    public AudioClip enemyDestroyed;
    public AudioClip collectiblePickup;
    public AudioClip gameOver;

    private void Awake()
    {
        // Enforce a Single Instance (Singleton)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep music playing across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Start background music automatically
        PlayMusic(backgroundMusic);
    }

    // Call this for one-shot sound effects (lasers, explosions, etc.)
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // Call this to change or start background music
    public void PlayMusic(AudioClip clip)
    {
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }
}
