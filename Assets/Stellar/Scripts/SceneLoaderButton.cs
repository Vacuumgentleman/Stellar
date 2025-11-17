using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneLoaderButton : MonoBehaviour
{
    [Header("Scene to Load (Editor Only)")]
#if UNITY_EDITOR
    public SceneAsset sceneAsset;
#endif

    [SerializeField, HideInInspector]
    private string sceneName;

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (sceneAsset != null)
        {
            string path = AssetDatabase.GetAssetPath(sceneAsset);
            sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
        }
#endif
    }

    private void Awake()
    {
        Button button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(LoadScene);
    }

    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("SceneLoaderButton: No scene assigned.");
        }
    }
}
