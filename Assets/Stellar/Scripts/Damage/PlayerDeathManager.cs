using UnityEngine;
using System.Collections;

public class PlayerDeathManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameObject mainObjectsToDisable;
    [SerializeField] private GameObject deathCamera;
    [SerializeField] private GameObject deathCanvas;

    [Header("Optional Objects")]
    [SerializeField] private GameObject enemyManager;
    [SerializeField] private GameObject pauseManager;

    [Header("Settings")]
    [SerializeField] private float cameraSwitchDelay = 2f;

    private bool hasProcessedDeath = false;

    private void Start()
    {
        SetInitialState();
    }

    private void Update()
    {
        if (hasProcessedDeath || playerHealth == null) return;

        if (playerHealth.IsDead)
        {
            hasProcessedDeath = true;
            StartCoroutine(DeathTransitionRoutine());
        }
    }

    private void SetInitialState()
    {
        mainObjectsToDisable?.SetActive(true);
        deathCamera?.SetActive(false);
        deathCanvas?.SetActive(false);
        enemyManager?.SetActive(true);
        pauseManager?.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private IEnumerator DeathTransitionRoutine()
    {
        yield return new WaitForSeconds(cameraSwitchDelay);

        mainObjectsToDisable?.SetActive(false);
        deathCamera?.SetActive(true);
        deathCanvas?.SetActive(true);
        enemyManager?.SetActive(false);
        pauseManager?.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
