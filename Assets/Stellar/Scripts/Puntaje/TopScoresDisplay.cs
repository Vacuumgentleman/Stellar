using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TopScoresDisplay : MonoBehaviour
{
    [Header("UI para Top 3")]
    public RawImage[] iconos;
    public TMP_Text[] nombres;
    public TMP_Text[] puntajes;
    public Texture[] imagenesDisponibles;

    // Conexion a MongoDB
    private const string connectionString = "mongodb+srv://StellarAdmin:tdF671iP1QOrmcWC@stellar.kdo26l7.mongodb.net/?retryWrites=true&w=majority&appName=Stellar";
    private MongoClient client;
    private IMongoDatabase database;
    private IMongoCollection<BsonDocument> collection;

    async void Start()
    {
        // Inicializar cliente y coleccion
        client = new MongoClient(connectionString);
        database = client.GetDatabase("StellarDataBase");
        collection = database.GetCollection<BsonDocument>("Puntajes");

        // Cargar los puntajes mas altos
        await CargarTopPuntajes();
    }

    async Task CargarTopPuntajes()
    {
        try
        {
            // Obtener los 3 puntajes mas altos, ordenados de mayor a menor
            var sort = Builders<BsonDocument>.Sort.Descending("puntaje");
            var topDocs = await collection.Find(new BsonDocument()).Sort(sort).Limit(3).ToListAsync();

            for (int i = 0; i < topDocs.Count && i < iconos.Length; i++)
            {
                var doc = topDocs[i];
                string nombre = doc.GetValue("nombre", "SinNombre").AsString;
                int puntaje = doc.GetValue("puntaje", 0).ToInt32();

                // Obtener el valor del icono como texto y convertirlo a entero
                string iconoStr = doc.GetValue("icono", "0").AsString;
                int iconoIndex = 0;
                int.TryParse(iconoStr, out iconoIndex);

                // Mostrar los datos en la interfaz
                nombres[i].text = nombre;
                puntajes[i].text = $"{puntaje} pts";

                // Asignar la textura si el indice es valido
                if (iconoIndex >= 0 && iconoIndex < imagenesDisponibles.Length)
                    iconos[i].texture = imagenesDisponibles[iconoIndex];
                else
                    iconos[i].texture = null;
            }
        }
        catch (System.Exception ex)
        {
            // Mostrar error en consola si falla la carga
            Debug.LogError("Error al cargar puntajes: " + ex.Message);
        }
    }
}
