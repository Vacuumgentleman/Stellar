using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrightAngle.Waypoint;

public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int poolSize = 10;
    [SerializeField] private int enemiesPerWave = 5;
    [SerializeField] private float timeBetweenWaves = 10f;
    [SerializeField] private float maxWaveDuration = 30f;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Enemy Pool Parent (Optional)")]
    [SerializeField] private Transform enemyPoolParent;

    private List<GameObject> enemyPool = new List<GameObject>();
    private int enemiesAlive = 0;
    private float waveTimer = 0f;
    private bool waveActive = false;

    private void Start()
    {
        CreatePool();
        StartCoroutine(WaveRoutine());
    }

    private void Update()
    {
        if (!waveActive) return;

        waveTimer += Time.deltaTime;

        if (enemiesAlive <= 0 || waveTimer >= maxWaveDuration)
        {
            StartCoroutine(WaveRoutine());
        }
    }

    private void CreatePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab);

            if (enemyPoolParent != null)
                enemy.transform.SetParent(enemyPoolParent);

            if (enemy.TryGetComponent(out Health health))
            {
                health.OnDeath.AddListener(() =>
                {
                    enemiesAlive--;

                    WaypointTarget waypoint = enemy.GetComponentInChildren<WaypointTarget>();
                    if (waypoint != null)
                        waypoint.DeactivateWaypoint();
                });
            }

            enemy.SetActive(false);
            enemyPool.Add(enemy);
        }
    }

    private IEnumerator WaveRoutine()
    {
        waveActive = false;
        waveTimer = 0f;

        yield return new WaitForSeconds(timeBetweenWaves);

        int spawned = 0;

        foreach (GameObject enemy in enemyPool)
        {
            if (enemy == null || enemy.activeInHierarchy) continue;
            if (spawned >= enemiesPerWave) break;

            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            enemy.transform.position = spawnPoint.position;
            enemy.transform.rotation = spawnPoint.rotation;

            if (enemy.TryGetComponent(out Health health))
                health.ResetHealth();

            enemy.SetActive(true);
            StartCoroutine(DelayedWaypointActivation(enemy));

            spawned++;
            enemiesAlive++;
        }

        waveActive = true;
    }

    private IEnumerator DelayedWaypointActivation(GameObject enemy)
    {
        yield return null;
        yield return null;

        WaypointTarget waypoint = enemy.GetComponentInChildren<WaypointTarget>();

        if (enemy.activeInHierarchy && waypoint != null && !waypoint.IsRegistered)
        {
            waypoint.ActivateWaypoint();
        }
    }
}
