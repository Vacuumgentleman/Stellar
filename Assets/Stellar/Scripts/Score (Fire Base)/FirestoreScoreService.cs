using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;

public class FirestoreScoreService : MonoBehaviour
{
    public static FirestoreScoreService Instance { get; private set; }

    private FirebaseFirestore db;
    private const string ScoreCollection = "StellarScores";

    private void Awake()
    {
        InitializeSingleton();
        InitializeDatabase();
    }

    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void InitializeDatabase()
    {
        try
        {
            db = FirebaseFirestore.DefaultInstance;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FirestoreScoreService] Firestore init failed: {ex.Message}");
            db = null;
        }
    }

    public async Task<bool> UploadScoreAsync(string playerName, int iconNumberOneBased, int score, string sceneName)
    {
        if (db == null)
        {
            Debug.LogError("[FirestoreScoreService] Firestore instance is null.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(playerName))
        {
            Debug.LogWarning("[FirestoreScoreService] Player name empty; using 'Unknown'.");
            playerName = "Unknown";
        }

        // Validate iconNumber range (1..7 expected)
        iconNumberOneBased = Mathf.Clamp(iconNumberOneBased, 1, 7);

        var data = new Dictionary<string, object>
        {
            { "PlayerName", playerName },
            { "Score", score },
            { "IconNumber", iconNumberOneBased },
            { "SceneName", sceneName ?? string.Empty },
            { "Timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
        };

        try
        {
            await db.Collection(ScoreCollection).AddAsync(data);
            Debug.Log("[FirestoreScoreService] Score uploaded.");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FirestoreScoreService] Upload failed: {ex.Message}");
            return false;
        }
    }

    public async Task<List<Dictionary<string, object>>> GetTopScoresAsync(int limit = 3)
    {
        var results = new List<Dictionary<string, object>>();

        if (db == null)
        {
            Debug.LogError("[FirestoreScoreService] Firestore instance is null.");
            return results;
        }

        try
        {
            var query = db.Collection(ScoreCollection)
                          .OrderByDescending("Score")
                          .Limit(limit);

            var snapshot = await query.GetSnapshotAsync();

            foreach (var doc in snapshot.Documents)
            {
                // doc.ToDictionary() returns object values — caller will parse them
                results.Add(doc.ToDictionary());
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FirestoreScoreService] GetTopScores failed: {ex.Message}");
        }

        return results;
    }
}
