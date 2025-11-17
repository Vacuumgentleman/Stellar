using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrightAngle.Waypoint;

public class EnemyManager : MonoBehaviour
{
    [Header("Configuracion del enemigo")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int poolSize = 10;
    [SerializeField] private int enemiesPerWave = 5;
    [SerializeField] private float timeBetweenWaves = 10f;
    [SerializeField] private float maxWaveDuration = 30f;

    [Header("Zonas de aparicion")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Contenedor de enemigos (opcional)")]
    [SerializeField] private Transform enemyPoolParent;

    private List<GameObject> enemyPool = new List<GameObject>();
    private int enemiesAlive = 0;
    private float waveTimer = 0f;
    private bool waveActive = false;

    private void Start()
    {
        CreatePool(); // Crear el grupo de enemigos
        StartCoroutine(WaveRoutine()); // Iniciar la rutina de oleadas
    }

    private void Update()
    {
        if (!waveActive) return;

        waveTimer += Time.deltaTime;

        // Si no hay enemigos vivos o se supero el tiempo maximo de oleada, iniciar una nueva
        if (enemiesAlive <= 0 || waveTimer >= maxWaveDuration)
        {
            StartCoroutine(WaveRoutine());
        }
    }

    // Crea la piscina (pool) de enemigos inactivos
    private void CreatePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab);

            if (enemyPoolParent != null)
                enemy.transform.SetParent(enemyPoolParent);

            // Si el enemigo tiene componente Health, registrar evento de muerte
            if (enemy.TryGetComponent(out Health health))
            {
                health.onDeath.AddListener(() =>
                {
                    enemiesAlive--;

                    // Desactiva el marcador de Waypoint si existe
                    WaypointTarget waypoint = enemy.GetComponentInChildren<WaypointTarget>();
                    if (waypoint != null)
                    {
                        waypoint.DeactivateWaypoint();
                    }
                });
            }

            enemy.SetActive(false); // Desactivar inicialmente
            enemyPool.Add(enemy);
        }
    }

    // Rutina para controlar la aparicion de enemigos por oleadas
    private IEnumerator WaveRoutine()
    {
        waveActive = false;
        waveTimer = 0f;

        yield return new WaitForSeconds(timeBetweenWaves); // Espera entre oleadas

        int spawned = 0;

        foreach (GameObject enemy in enemyPool)
        {
            if (enemy == null || enemy.activeInHierarchy) continue;
            if (spawned >= enemiesPerWave) break;

            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            enemy.transform.position = spawnPoint.position;
            enemy.transform.rotation = spawnPoint.rotation;

            // Reiniciar salud si tiene el componente
            if (enemy.TryGetComponent(out Health health))
            {
                health.ResetHealth();
            }

            enemy.SetActive(true); // Activar el enemigo
            StartCoroutine(DelayedWaypointActivation(enemy)); // Activar Waypoint despues

            spawned++;
            enemiesAlive++;
        }

        waveActive = true;
    }

    // Activa el waypoint del enemigo despues de un breve retraso
    private IEnumerator DelayedWaypointActivation(GameObject enemy)
    {
        yield return null;
        yield return null;

        WaypointTarget waypoint = enemy.GetComponentInChildren<WaypointTarget>();

        if (enemy.activeInHierarchy && waypoint != null)
        {
            if (!waypoint.IsRegistered)
            {
                waypoint.ActivateWaypoint(); // Activar el waypoint si no esta registrado
            }
        }
    }
}
