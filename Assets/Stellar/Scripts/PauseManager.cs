using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PauseManager : MonoBehaviour
{
    [Header("Teclas de control")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private KeyCode confirmKey = KeyCode.Return;

    [Header("UI de pausa")]
    [SerializeField] private GameObject pauseMenuUI;

    [Header("Escena de menú principal")]
#if UNITY_EDITOR
    [SerializeField] private SceneAsset menuSceneAsset; // <-- Puedes arrastrar la escena aquí
#endif
    [SerializeField, HideInInspector] private string menuSceneName;

    private bool isPaused = false;

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
        // Restaurar el tiempo por si acaso
        Time.timeScale = 1f;
        isPaused = false;

        // Ocultar la UI de pausa antes de cambiar de escena
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Cargar la escena
        if (!string.IsNullOrEmpty(menuSceneName))
        {
            SceneManager.LoadScene(menuSceneName);
        }
        else
        {
            Debug.LogWarning("No se ha asignado una escena de menú en PauseManager.");
        }
    }


    public bool IsPaused()
    {
        return isPaused;
    }
}
