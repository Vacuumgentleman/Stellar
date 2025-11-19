using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Final UI (same scene)")]
    [SerializeField] private GameObject gameOverPanel;         // panel to enable when player dies
    [SerializeField] private TMP_InputField nameInput;         // player name input
    [SerializeField] private IconSelector iconSelector;        // reference to icon selector in the game over panel

    [Header("Menu Scene")]
#if UNITY_EDITOR
    [SerializeField] private SceneAsset menuSceneAsset;
#endif
    [SerializeField, HideInInspector] private string menuSceneName;

    private int currentScore;
    private bool isActive = true;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (menuSceneAsset != null)
            menuSceneName = menuSceneAsset.name;
#endif
    }

    private void InitializeSingleton()
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
        if (scoreText != null)
            scoreText.text = "0";
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void Update()
    {
        if (!isActive) return;
        // Score text updates when score changes; leave Update minimal.
    }

    /// <summary>
    /// Called by gameplay logic when an enemy ship is destroyed.
    /// </summary>
    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreDisplay();
    }

    // -------------------------------------------------------
    // REQUIRED BY Health.cs – DO NOT REMOVE OR RENAME
    // -------------------------------------------------------
    public void AddEnemyKillScore()
    {
        AddScore(10);
    }

    /// <summary>
    /// Called by health/other scripts when the player dies.
    /// Activates the game over UI (same scene).
    /// </summary>
    public void OnPlayerDeath()
    {
        isActive = false;
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
            scoreText.text = currentScore.ToString();
    }

    public int GetFinalScore() => currentScore;

    public void ResetScore()
    {
        currentScore = 0;
        isActive = true;
        UpdateScoreDisplay();
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    /// <summary>
    /// Called by the Upload button in the Game Over UI.
    /// </summary>
    public async void UploadScoreAndReturnToMenu()
    {
        string playerName = nameInput != null ? nameInput.text.Trim() : string.Empty;
        if (string.IsNullOrEmpty(playerName))
            playerName = "Unknown";

        int iconNumberOneBased = iconSelector != null ? iconSelector.GetSelectedNumberOneBased() : 1;

        if (string.IsNullOrEmpty(menuSceneName))
        {
            Debug.LogError("[ScoreManager] Menu scene name is not set. Drag the scene to the Menu Scene Asset field.");
            return;
        }

        if (FirestoreScoreService.Instance == null)
        {
            Debug.LogError("[ScoreManager] FirestoreScoreService is missing in the scene.");
            return;
        }

        bool success = await FirestoreScoreService.Instance.UploadScoreAsync(playerName, iconNumberOneBased, currentScore, menuSceneName);

        if (success)
        {
            await Task.Delay(800);
            SceneManager.LoadScene(menuSceneName);
        }
        else
        {
            Debug.LogError("[ScoreManager] Failed to upload score. Stay in Game Over UI.");
        }
    }
}
