using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Button confirmButton;

    [Header("Icon Selector")]
    [SerializeField] private RawImage iconDisplay;
    [SerializeField] private Texture[] icons;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;

    [Header("Main Menu Scene")]
#if UNITY_EDITOR
    [SerializeField] private SceneAsset menuSceneAsset;
#endif
    [SerializeField, HideInInspector] private string menuSceneName;

    [Header("Score Settings")]
    [SerializeField] private int totalScore = 0;
    [SerializeField] private int scorePerEnemy = 150;

    private int currentIconIndex = 0;

    // MongoDB Settings
    private const string ConnectionString = 
        "mongodb+srv://StellarAdmin:tdF671iP1QOrmcWC@stellar.kdo26l7.mongodb.net/?retryWrites=true&w=majority&appName=Stellar";

    private MongoClient client;
    private IMongoDatabase database;
    private IMongoCollection<BsonDocument> collection;

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (menuSceneAsset != null)
            menuSceneName = menuSceneAsset.name;
#endif
    }

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
        InitializeDatabase();
        AssignButtonEvents();
        RefreshIcon();
        UpdateScoreText();
    }

    private void InitializeDatabase()
    {
        client = new MongoClient(ConnectionString);
        database = client.GetDatabase("StellarDataBase");
        collection = database.GetCollection<BsonDocument>("Puntajes");
    }

    private void AssignButtonEvents()
    {
        previousButton.onClick.AddListener(ShowPreviousIcon);
        nextButton.onClick.AddListener(ShowNextIcon);
        confirmButton.onClick.AddListener(() => StartCoroutine(SaveAndReturnToMenu()));
    }

    // -------------------------
    // Score Management
    // -------------------------

    public void AddEnemyKillScore()
    {
        totalScore += scorePerEnemy;
        UpdateScoreText();
        Debug.Log($"[ScoreManager] Added {scorePerEnemy} points. Total = {totalScore}");
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {totalScore:00000}";
    }

    public int GetTotalScore() => totalScore;

    public void ResetScore()
    {
        totalScore = 0;
        UpdateScoreText();
    }

    // -------------------------
    // Icon Selector
    // -------------------------

    private void ShowPreviousIcon()
    {
        currentIconIndex = (currentIconIndex - 1 + icons.Length) % icons.Length;
        RefreshIcon();
    }

    private void ShowNextIcon()
    {
        currentIconIndex = (currentIconIndex + 1) % icons.Length;
        RefreshIcon();
    }

    private void RefreshIcon()
    {
        if (icons.Length > 0)
            iconDisplay.texture = icons[currentIconIndex];
    }

    // -------------------------
    // Save and Return
    // -------------------------

    private IEnumerator SaveAndReturnToMenu()
    {
        string playerName = nameInput.text.Trim();
        string icon = currentIconIndex.ToString();

        if (string.IsNullOrWhiteSpace(playerName))
        {
            Debug.LogWarning("Name field is required.");
            yield break;
        }

        Task<bool> saveTask = SaveToMongoDB(playerName, icon, totalScore);
        yield return new WaitUntil(() => saveTask.IsCompleted);

        if (saveTask.Result)
        {
            yield return new WaitForSecondsRealtime(1f);
            Time.timeScale = 1f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (!string.IsNullOrEmpty(menuSceneName))
                SceneManager.LoadScene(menuSceneName);
            else
                Debug.LogWarning("ScoreManager: No main menu scene assigned.");
        }
    }

    private async Task<bool> SaveToMongoDB(string name, string icon, int score)
    {
        var document = new BsonDocument
        {
            { "name", name },
            { "icon", icon },
            { "score", score }
        };

        try
        {
            await collection.InsertOneAsync(document);
            Debug.Log("Score saved to MongoDB Atlas.");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("MongoDB save error: " + ex.Message);
            return false;
        }
    }
}
