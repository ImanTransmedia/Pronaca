using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderAdditive : MonoBehaviour
{
    [Header("Nombre de la escena que se cargará en aditivo")]
    public string sceneToLoad;

    [Header("Opcional: Descargar al cerrar")]
    public bool unloadOnDisable = false;

    private AsyncOperation loadOperation;

    void Start()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            LoadAdditiveScene();
        }
    }

    public void LoadAdditiveScene()
    {
        if (SceneManager.GetSceneByName(sceneToLoad).isLoaded)
        {
            Debug.Log($"La escena {sceneToLoad} ya está cargada.");
            return;
        }

        loadOperation = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        loadOperation.completed += (AsyncOperation op) =>
        {
            Debug.Log($"Escena {sceneToLoad} cargada en modo aditivo.");
        };
    }

    public void UnloadAdditiveScene()
    {
        if (SceneManager.GetSceneByName(sceneToLoad).isLoaded)
        {
            SceneManager.UnloadSceneAsync(sceneToLoad);
            Debug.Log($"Escena {sceneToLoad} descargada.");
        }
    }

    private void OnDisable()
    {
        if (unloadOnDisable)
        {
            UnloadAdditiveScene();
        }
    }
}
