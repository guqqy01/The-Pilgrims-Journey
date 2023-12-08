using UnityEngine;
using UnityEngine.SceneManagement;
using Map;
using UnityEngine.Events;

public class GlobalMapPoint : MonoBehaviour, InputInteractiveObject
{
    public LocalMapGenerationSettings localMapSettings;

    private new Collider2D collider2D;

    public Collider2D Collider { get => collider2D; }

    private void Awake()
    {
        collider2D = GetComponent<Collider2D>();
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        GameManager.Instance.SetupLocalMap();
    }

    public void Interact()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        GameManager.Instance.SetLocalMapObject(localMapSettings);
        SceneManager.LoadSceneAsync("LocalMap");
    }
}
