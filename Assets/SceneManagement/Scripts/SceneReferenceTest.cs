#if UNITY_EDITOR
#endif
using UnityEngine;
using System.Collections.Generic;
public class SceneReferenceTest : MonoBehaviour
{
    public List<SceneReference> sceneReference;

    private void OnValidate()
    {
        foreach (SceneReference sceneReference in sceneReference)
        {
            if (sceneReference != null)
                sceneReference.OnValidate();
        }
    }
}
