using UnityEngine;
using UnityEngine.SceneManagement;
using Map;
using UnityEngine.Events;

public class GlobalMapPoint : MonoBehaviour
{
    public LocalMapGenerationSettings localMapSettings;

    [SerializeField]
    private Collider2D collider2D;

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
        SceneManager.LoadScene("LocalMap");
        SceneManager.sceneLoaded += Test;
        SceneManager.sceneUnloaded += Test;
        LocalMapGenerator.Instance.biomeSettings = localMapSettings;
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
