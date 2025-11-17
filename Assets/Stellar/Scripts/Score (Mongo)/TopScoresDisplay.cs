using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TopScoresDisplay : MonoBehaviour
{
    [Header("UI - Top 3")]
    [SerializeField] private RawImage[] iconSlots;
    [SerializeField] private TMP_Text[] nameLabels;
    [SerializeField] private TMP_Text[] scoreLabels;
    [SerializeField] private Texture[] availableIcons;

    private const string ConnectionString =
        "mongodb+srv://StellarAdmin:tdF671iP1QOrmcWC@stellar.kdo26l7.mongodb.net/?retryWrites=true&w=majority&appName=Stellar";

    private MongoClient client;
    private IMongoDatabase database;
    private IMongoCollection<BsonDocument> collection;

    private async void Start()
    {
        InitializeDatabase();
        await LoadTopScores();
    }

    private void InitializeDatabase()
    {
        client = new MongoClient(ConnectionString);
        database = client.GetDatabase("StellarDataBase");
        collection = database.GetCollection<BsonDocument>("Puntajes");
    }

    private async Task LoadTopScores()
    {
        try
        {
            var sort = Builders<BsonDocument>.Sort.Descending("score");
            var topDocs = await collection
                .Find(new BsonDocument())
                .Sort(sort)
                .Limit(3)
                .ToListAsync();

            for (int i = 0; i < topDocs.Count && i < iconSlots.Length; i++)
            {
                var doc = topDocs[i];

                string name = doc.GetValue("name", "Unknown").AsString;
                int score = doc.GetValue("score", 0).ToInt32();
                string iconStr = doc.GetValue("icon", "0").AsString;

                int iconIndex = int.TryParse(iconStr, out int parsed) ? parsed : 0;

                nameLabels[i].text = name;
                scoreLabels[i].text = $"{score} pts";

                if (iconIndex >= 0 && iconIndex < availableIcons.Length)
                    iconSlots[i].texture = availableIcons[iconIndex];
                else
                    iconSlots[i].texture = null;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to load scores: " + ex.Message);
        }
    }
}
