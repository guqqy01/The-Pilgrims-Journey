using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
[CreateAssetMenu(menuName = "Scene/Scene Load Item")]
public class SceneLoadItem : ScriptableObject
{
    public SceneReference activeScene;
    public List<SceneReference> otherScenesToBeLoaded;

    public bool UseLoadingScreen = false;

    public List<string> SceneNames
    {
        get
        {
            List<string> returnList = new List<string>() { activeScene.sceneName};

            for (int i = 0; i < otherScenesToBeLoaded.Count; i++)
            {
                if (!returnList.Contains(otherScenesToBeLoaded[i].sceneName))
                    returnList.Add(otherScenesToBeLoaded[i].sceneName);
            }

            return returnList;
        }
    }
}
