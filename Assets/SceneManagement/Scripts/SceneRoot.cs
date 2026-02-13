using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UIElements;
using System.Reflection;


public class SceneRoot : MonoBehaviour
{
    private SceneLoader loader;

    private static List<Action> waitForCommonSceneToLoadActions = new List<Action>();

    [SerializeField]
    private SceneReference commonScene;
    public static bool CommonSceneLoaded { set; get; } = false;

    void Awake()
    {
        loader = SceneLoader.Instance;

        ISingletonToBeInitiatedFromRoot[] singletonsFromChildren = GetComponentsInChildren<ISingletonToBeInitiatedFromRoot>(true);

        foreach (var singleton in singletonsFromChildren)
        {
            singleton.InitiateSingleton();
        }


        if (!CommonSceneLoaded)
        {
            StartCoroutine(LoadCommonSceneIfNotLoaded());
            gameObject.SetActive(false);
        }
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (commonScene != null)
            commonScene.OnValidate();
    }
#endif
    public static Scene[] GetAllScenes()
    {
        Scene[] returnList = new Scene[SceneManager.sceneCount];

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            returnList[i] = SceneManager.GetSceneAt(i);
        }

        return returnList;
    }

    


    public IEnumerator LoadCommonSceneIfNotLoaded()
    {
        if (!CommonSceneLoaded)
        {
            CommonSceneLoaded = true;
            Scene[] scenes = GetAllScenes();

            bool loadCommonScene = true;

            foreach (Scene scene in GetAllScenes())
            {
                if (scene.name == commonScene.sceneName)
                {
                    loadCommonScene = false;
                }
            }

            if (loadCommonScene)
            {
                yield return StartCoroutine(LoadSceneAdditive(commonScene.sceneName));

                yield return null;

                foreach(Action action in waitForCommonSceneToLoadActions)
                {
                    action.Invoke();
                }
            }
        }
    }

    public static void DoActionAfterCommonSceneLoaded(Action action)
    {
        if (CommonSceneLoaded)
        {
            action.Invoke();
        }
        else
        {
            waitForCommonSceneToLoadActions.Add(action);
        }
    }

    public IEnumerator LoadSceneAdditive(string sceneToLoad)
    {
        bool sceneAlreadyLoaded = false;

        foreach (Scene scene in GetAllScenes())
        {
            if (scene.name == sceneToLoad)
            {
                sceneAlreadyLoaded = true;
            }
        }

        if (!sceneAlreadyLoaded)
            yield return SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
    }
}
