using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
[CreateAssetMenu()]
public class SceneLoadItem : ScriptableObject
{
    public SceneAsset activeScene;
    public List<SceneAsset> otherScenesToBeLoaded;

    public bool UseLoadingScreen = false;

    public List<string> SceneNames
    {
        get
        {
            List<string> returnList = new List<string>() { activeScene.name};

            for (int i = 0; i < otherScenesToBeLoaded.Count; i++)
            {
                if (!returnList.Contains(otherScenesToBeLoaded[i].name))
                    returnList.Add(otherScenesToBeLoaded[i].name);
            }

            return returnList;
        }
    }
}
