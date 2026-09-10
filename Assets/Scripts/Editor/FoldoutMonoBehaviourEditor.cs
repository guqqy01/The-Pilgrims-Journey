using UnityEditor;
using UnityEngine;
/// <summary>
/// Generic editor for any MonoBehaviour. Only takes over drawing if the
/// component actually has [Foldout] fields; otherwise it falls back to the
/// default inspector so it never changes the look of components you haven't
/// tagged.
///
/// NOTE: Unity only allows one CustomEditor covering a given type hierarchy.
/// If you already have another package/editor also doing
/// [CustomEditor(typeof(MonoBehaviour), true)], this will conflict with it —
/// in that case, call FoldoutGroupDrawer.DrawInspectorWithFoldouts(serializedObject)
/// directly from inside that other editor's OnInspectorGUI() instead of using
/// this class.
/// </summary>
[CustomEditor(typeof(MonoBehaviour), true)]
[CanEditMultipleObjects]
public class FoldoutMonoBehaviourEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (FoldoutGroupDrawer.HasFoldoutFields(target))
            FoldoutGroupDrawer.DrawInspectorWithFoldouts(serializedObject);
        else
            base.OnInspectorGUI();
    }
}
