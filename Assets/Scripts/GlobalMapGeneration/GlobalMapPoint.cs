using UnityEngine;
using UnityEngine.SceneManagement;
using Map;
using UnityEngine.Events;

public class GlobalMapPoint : MonoBehaviour
{
    public LocalMapGenerationSettings localMapSettings;

    private new Collider2D collider2D;

    private void Awake()
    {
        collider2D = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics2D.OverlapPoint(ray.origin) == collider2D)
            LoadLocalMap();
    }

    public void LoadLocalMap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        GameManager.Instance.SetLocalMapObject(localMapSettings);
        SceneManager.LoadSceneAsync("LocalMap");
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        GameManager.Instance.SetupLocalMap();
    }
}
