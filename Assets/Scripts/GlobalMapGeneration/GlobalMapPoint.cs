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
        SceneManager.sceneLoaded += Test;
        SceneManager.sceneUnloaded += Test;
        SceneManager.LoadScene("LocalMap");
        LocalMapGenerator.Instance.generationSettings = localMapSettings;
        LocalMapGenerator.Instance.CreateMap();
    }

    public void Test(Scene scene, LoadSceneMode loadSceneMode)
    {
        Debug.Log("Loaded: " + scene.name);
    }

    public void Test(Scene scene)
    {
        Debug.Log("Unloaded: " + scene.name);
    }
}
