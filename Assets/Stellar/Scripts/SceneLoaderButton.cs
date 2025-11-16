using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneLoaderButton : MonoBehaviour
{
    [Header("Asigna la escena desde el editor")]
#if UNITY_EDITOR
    public SceneAsset sceneToLoad;  // Esto solo existe en el editor
#endif

    [SerializeField, HideInInspector]
    private string sceneName;

    private void OnValidate()
    {
        // Esto se ejecuta solo en el editor, para obtener el nombre de la escena
#if UNITY_EDITOR
        if (sceneToLoad != null)
        {
            string path = AssetDatabase.GetAssetPath(sceneToLoad);
            sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
        }
#endif
    }

    private void Awake()
    {
        // Asegura que el botón tenga el evento asignado
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(LoadScene);
        }
    }

    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("No se ha asignado ninguna escena para cargar.");
        }
    }
}
