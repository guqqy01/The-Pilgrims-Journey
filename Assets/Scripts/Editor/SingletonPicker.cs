using UnityEditor;
using UnityEngine;

public class SingletonPicker : EditorWindow
{
    private SerializedProperty property;

    public static void Show(SerializedProperty prop)
    {
        var window = CreateInstance<SingletonPicker>();
        window.property = prop;
        window.ShowAuxWindow();
    }

    void OnGUI()
    {
        foreach (var singleton in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (singleton is ISingletonToBeInitiatedFromRoot)
            {
                if (GUILayout.Button(singleton.name))
                {
                    property.objectReferenceValue = singleton;
                    property.serializedObject.ApplyModifiedProperties();
                    Close();
                }
            }
        }
    }
}