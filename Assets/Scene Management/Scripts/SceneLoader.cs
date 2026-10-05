using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Scene = UnityEngine.SceneManagement.Scene;
using System.Linq;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    [SerializeField]
    private List<SceneReference> dontUnloadScenes;

    [SerializeField]
    private SceneManagementSettings sceneSettings;

    [SerializeField]
    private Animator animator;

    public static bool InitialSetupDone { private set; get; } = false;

    private Dictionary<Scene, SceneRoot> scenesAndTheirRoots = new Dictionary<Scene, SceneRoot>();

    private void Awake()
    {
        Instance = this;

        if (!InitialSetupDone)
            InitialSetup();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        foreach(SceneReference sceneReference in dontUnloadScenes)
        {
            if(sceneReference != null)
                sceneReference.OnValidate();
        }
    }
#endif
    public void InitialSetup()
    {
        if (!InitialSetupDone)
        {
            LoadSceneFromType(SceneLoad.MainMenu, () => InitialSetupDone = true);
            SceneRoot.CommonSceneLoaded = true;
        }
    }


    private static SceneRoot GetSceneRoot(Scene scene)
    {
        GameObject[] rootObjects = scene.GetRootGameObjects();

        foreach(GameObject rootObject in rootObjects)
        {
            if (rootObject.TryGetComponent(out SceneRoot sceneRoot))
            {
                return sceneRoot;
            }
        }

        Debug.LogWarning($"Scene: {scene}, does not contain a SceneRoot object.");
        return null;
    }

    private static bool TryGetSceneRoot(Scene scene, out SceneRoot sceneRoot)
    {
        GameObject[] rootObjects = scene.GetRootGameObjects();

        foreach (GameObject rootObject in rootObjects)
        {
            if (rootObject.TryGetComponent(out sceneRoot))
            {
                return true;
            }
        }

        sceneRoot = null;
        return false;
    }

    public static Scene[] GetAllScenes()
    {
        Scene[] returnList = new Scene[SceneManager.sceneCount];

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            returnList[i] = SceneManager.GetSceneAt(i);
        }

        return returnList;
    }

    public static Scene GetScene(string sceneName)
    {
        foreach(Scene scene in GetAllScenes())
        {
            if(scene.name == sceneName) return scene;
        }

        throw new Exception($"Cant find scene: {sceneName}");
    }

    public static string[] GetAllSceneNames()
    {
        string[] returnList = new string[SceneManager.sceneCount];

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            returnList[i] = SceneManager.GetSceneAt(i).name;
        }

        return returnList;
    }

    public void LoadSceneFromType(SceneLoad sceneLoad, Action onSceneLoaded = null)
    {
        StartCoroutine(LoadSceneItem(sceneSettings.GetScenLoadItem(sceneLoad), onSceneLoaded));
    }

    private IEnumerator LoadSceneItem(SceneLoadItem sceneLoadItem, Action onSceneLoaded = null)
    {
        if (sceneLoadItem.UseLoadingScreen)
        {
            yield return LoadSceneItemWithLoadingScreen(sceneLoadItem, onSceneLoaded);
        }
        else
        {
            yield return LoadSceneItemNoLoadingScreen(sceneLoadItem, onSceneLoaded);
        }
    }
    
    private IEnumerator FadeToBlack()
    {
        if (animator != null)
        {
            animator.SetTrigger("FadeToBlack");
            animator.ResetTrigger("FadeFromBlack");
            yield return new WaitForSecondsRealtime(1f);
        }
    }

    private IEnumerator FadeFromBlack()
    {
        if (animator != null)
        {
            animator.SetTrigger("FadeFromBlack");
            animator.ResetTrigger("FadeToBlack");
            yield return new WaitForSecondsRealtime(1f);
        }
    }

    private IEnumerator LoadSceneItemWithLoadingScreen(SceneLoadItem sceneLoadItem, Action onSceneLoaded = null)
    {
        yield return FadeToBlack();

        List<Scene> scenesToRemove = new List<Scene>();

        foreach (Scene scene in GetAllScenes())
        {
            if (!sceneLoadItem.SceneNames.Contains(scene.name) && !dontUnloadScenes.Select(x => x.sceneName).Contains(scene.name))
            {
                scenesToRemove.Add(scene);
            }
        }

        yield return StartCoroutine(DeactivateRootsOrUnloadScenes(scenesToRemove));

        string loadingSceneName = sceneSettings.GetScenLoadItem(SceneLoad.LoadingScreen).activeScene.sceneName;

        yield return StartCoroutine(LoadSceneAdditiveWithRootsDeactivated(loadingSceneName));
        Scene loadingScene = GetScene(loadingSceneName);
        ActivateSceneRoot(loadingScene);
        SceneManager.SetActiveScene(loadingScene);

        LoadingSceneBackgroundImage.Instance.SetRandomLoadingImage();

        yield return FadeFromBlack();

        foreach (string sceneToLoad in sceneLoadItem.SceneNames)
        {
            yield return StartCoroutine(LoadSceneAdditiveWithRootsDeactivated(sceneToLoad));
        }
        yield return new WaitForSecondsRealtime(0.5f);

        yield return FadeToBlack();

        yield return DeactivateRootsOrUnloadScenes(new List<Scene>(){ loadingScene });

        ActivateSceneRoots(GetScenesFromSceneLoadItem(sceneLoadItem));

        SceneManager.SetActiveScene(GetScene(sceneLoadItem.activeScene.sceneName));

        yield return null;

        try
        {
            onSceneLoaded?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        yield return FadeFromBlack();
    }

    private IEnumerator LoadSceneItemNoLoadingScreen(SceneLoadItem sceneLoadItem, Action onSceneLoaded = null)
    {
        yield return FadeToBlack();

        List<Scene> scenesToRemove = new List<Scene>();

        foreach (Scene scene in GetAllScenes())
        {
            if (!sceneLoadItem.SceneNames.Contains(scene.name) && !dontUnloadScenes.Select(x => x.sceneName).Contains(scene.name))
            {
                scenesToRemove.Add(scene);
            }
        }

        yield return StartCoroutine(DeactivateRootsOrUnloadScenes(scenesToRemove));

        foreach (string sceneToLoad in sceneLoadItem.SceneNames)
        {
            yield return StartCoroutine(LoadSceneAdditiveWithRootsDeactivated(sceneToLoad));
        }

        ActivateSceneRoots(GetScenesFromSceneLoadItem(sceneLoadItem));

        SceneManager.SetActiveScene(GetScene(sceneLoadItem.activeScene.sceneName));

        yield return null;

        try
        {
            onSceneLoaded?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        yield return FadeFromBlack();
    }

    private List<Scene> GetScenesFromSceneLoadItem(SceneLoadItem sceneLoadItem)
    {
        List<Scene> returnList = new List<Scene>();

        List<string> sceneNames = sceneLoadItem.SceneNames;

        foreach (Scene scene in GetAllScenes())
        {
            if (sceneNames.Contains(scene.name))
            {
                returnList.Add(scene);
            }
        }

        return returnList;
    }

    private IEnumerator DeactivateRootsOrUnloadScenes(List<Scene> scenesToRemove)
    {
        foreach (Scene scene in scenesToRemove)
        {
            if (dontUnloadScenes.Select(x => x.sceneName).Contains(scene.name))
                continue;

            if (scenesAndTheirRoots.ContainsKey(scene))
            {
                scenesAndTheirRoots[scene].gameObject.SetActive(false);
            }
            else if (scene.GetRootGameObjects()[0].TryGetComponent(out SceneRoot root))
            {
                scenesAndTheirRoots.Add(scene, root);
                scenesAndTheirRoots[scene].gameObject.SetActive(false);
            }
            else
            {
                yield return SceneManager.UnloadSceneAsync(scene);
            }
        }

        yield return null;
    }

    private IEnumerator LoadSceneAdditiveWithRootsDeactivated(string sceneToLoad)
    {
        bool sceneAlreadyLoaded = false;

        foreach(Scene scene in GetAllScenes())
        {
            if(scene.name == sceneToLoad)
            {
                sceneAlreadyLoaded = true;

                if (!scenesAndTheirRoots.ContainsKey(scene))
                {
                    if(TryGetSceneRoot(scene, out SceneRoot root))
                    {
                        scenesAndTheirRoots.Add(scene, root);
                    }
                }

                if (scenesAndTheirRoots.ContainsKey(scene))
                    scenesAndTheirRoots[scene].gameObject.SetActive(false);

                break;
            }
        }

        if (!sceneAlreadyLoaded)
        {
            yield return SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);

            Scene loadedScene = SceneManager.GetSceneByName(sceneToLoad);

            if (TryGetSceneRoot(loadedScene, out SceneRoot root))
            {
                scenesAndTheirRoots.Add(loadedScene, root);
                root.gameObject.SetActive(false);
            }
        }
    }

    private void ActivateSceneRoots(List<Scene> scenesToActivate)
    {
        foreach(Scene scene in scenesToActivate)
        {
            ActivateSceneRoot(scene);
        }
    }

    private void ActivateSceneRoot(Scene sceneToActivate)
    {
        if (!scenesAndTheirRoots.ContainsKey(sceneToActivate))
        {
            if (TryGetSceneRoot(sceneToActivate, out SceneRoot root))
            {
                scenesAndTheirRoots.Add(sceneToActivate, root);
            }
        }

        if (scenesAndTheirRoots.ContainsKey(sceneToActivate))
            scenesAndTheirRoots[sceneToActivate].gameObject.SetActive(true);
    }
}

public enum SceneLoad
{
    MainMenu,
    InGame,
    PhaseOneLoad,
    PhaseTwoLoad,
    Shop,
    RandomEvent,
    LoadingScreen,
    GlobalMap
}