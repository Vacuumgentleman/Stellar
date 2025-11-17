using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PauseManager : MonoBehaviour
{
    [Header("Input Keys")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private KeyCode confirmKey = KeyCode.Return;

    [Header("Pause UI")]
    [SerializeField] private GameObject pauseMenuUI;

    [Header("Main Menu Scene")]
#if UNITY_EDITOR
    [SerializeField] private SceneAsset menuSceneAsset;
#endif
    [SerializeField, HideInInspector] 
    private string menuSceneName;

    private bool isPaused;

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (menuSceneAsset != null)
            menuSceneName = menuSceneAsset.name;
#endif
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (!isPaused)
                Pause();
            else
                Resume();
        }

        if (isPaused && Input.GetKeyDown(confirmKey))
        {
            ReturnToMenu();
        }
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        isPaused = true;

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ReturnToMenu()
    {
        // Reset time and state
        Time.timeScale = 1f;
        isPaused = false;

        // Hide pause UI
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (!string.IsNullOrEmpty(menuSceneName))
        {
            SceneManager.LoadScene(menuSceneName);
        }
        else
        {
            Debug.LogWarning("PauseManager: No menu scene assigned.");
        }
    }

    public bool IsPaused() => isPaused;
}
