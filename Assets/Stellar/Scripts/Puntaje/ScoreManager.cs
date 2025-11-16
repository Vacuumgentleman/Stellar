using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;
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
    public TMP_InputField nombreInput;
    public TMP_Text puntajeTexto;
    public Button confirmarButton;

    [Header("Selector de Imagen")]
    public RawImage rawImage;
    public Texture[] images;
    public Button leftButton;
    public Button rightButton;

    [Header("Escena de menu principal")]
#if UNITY_EDITOR
    public SceneAsset menuSceneAsset;
#endif
    [HideInInspector] public string menuSceneName;

    [Header("Puntaje")]
    [SerializeField] private int totalScore = 0;
    [SerializeField] private int pointsPerEnemy = 150;

    private int currentIndex = 0;

    // Conexion a MongoDB
    private const string connectionString = "mongodb+srv://StellarAdmin:tdF671iP1QOrmcWC@stellar.kdo26l7.mongodb.net/?retryWrites=true&w=majority&appName=Stellar";
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

    void Awake()
    {
        // Implementacion de singleton global
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Inicializar conexion a MongoDB
        client = new MongoClient(connectionString);
        database = client.GetDatabase("StellarDataBase");
        collection = database.GetCollection<BsonDocument>("Puntajes");

        // Asignar funciones a botones
        leftButton.onClick.AddListener(PreviousImage);
        rightButton.onClick.AddListener(NextImage);
        confirmarButton.onClick.AddListener(() => StartCoroutine(ConfirmarYVolver()));

        // Cargar imagen inicial y texto de puntaje
        UpdateImage();
        ActualizarTextoPuntaje();
    }

    // Incrementar puntaje por enemigo eliminado
    public void AddEnemyKillScore()
    {
        totalScore += pointsPerEnemy;
        ActualizarTextoPuntaje();
        Debug.Log($"[ScoreManager] +{pointsPerEnemy} puntos. Total: {totalScore}");
    }

    // Actualizar el texto que muestra el puntaje actual
    private void ActualizarTextoPuntaje()
    {
        if (puntajeTexto != null)
            puntajeTexto.text = $"Puntaje: {totalScore:00000}";
    }

    // Obtener el puntaje total actual
    public int GetTotalScore() => totalScore;

    // Reiniciar puntaje
    public void ResetScore()
    {
        totalScore = 0;
        ActualizarTextoPuntaje();
    }

    // Cambiar a la imagen anterior
    void PreviousImage()
    {
        currentIndex = (currentIndex - 1 + images.Length) % images.Length;
        UpdateImage();
    }

    // Cambiar a la siguiente imagen
    void NextImage()
    {
        currentIndex = (currentIndex + 1) % images.Length;
        UpdateImage();
    }

    // Actualizar la textura mostrada
    void UpdateImage()
    {
        if (images.Length > 0)
            rawImage.texture = images[currentIndex];
    }

    // Confirmar datos y volver al menu principal
    private IEnumerator ConfirmarYVolver()
    {
        string nombre = nombreInput.text.Trim();
        string icono = currentIndex.ToString();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            Debug.LogWarning("El nombre es obligatorio.");
            yield break;
        }

        // Iniciar tarea para guardar datos en MongoDB
        Task<bool> guardarTask = GuardarEnMongoDB(nombre, icono, totalScore);
        yield return new WaitUntil(() => guardarTask.IsCompleted);

        // Si fue exitoso, volver al menu
        if (guardarTask.Result)
        {
            yield return new WaitForSecondsRealtime(1f);
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (!string.IsNullOrEmpty(menuSceneName))
            {
                SceneManager.LoadScene(menuSceneName);
            }
            else
            {
                Debug.LogWarning("No se ha asignado una escena de menu.");
            }
        }
    }

    // Guardar los datos en MongoDB de forma asincronica
    private async Task<bool> GuardarEnMongoDB(string nombre, string icono, int puntaje)
    {
        var document = new BsonDocument
        {
            { "nombre", nombre },
            { "icono", icono },
            { "puntaje", puntaje }
        };

        try
        {
            await collection.InsertOneAsync(document);
            Debug.Log("Datos guardados en MongoDB Atlas.");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error al guardar en MongoDB: " + ex.Message);
            return false;
        }
    }
}
