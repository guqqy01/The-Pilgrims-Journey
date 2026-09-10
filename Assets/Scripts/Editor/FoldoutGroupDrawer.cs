using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Shared logic for drawing an inspector where fields tagged with
/// [Foldout("GroupName")] are collected and drawn inside collapsible
/// foldout headers, and everything else is drawn normally above them.
/// </summary>
public static class FoldoutGroupDrawer
{
    public static void DrawInspectorWithFoldouts(SerializedObject serializedObject)
    {
        serializedObject.Update();

        var groupOrder = new List<string>();
        var groups = new Dictionary<string, List<SerializedProperty>>();
        var ungrouped = new List<SerializedProperty>();

        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (iterator.name == "m_Script")
                continue;

            FieldInfo field = GetFieldInfo(serializedObject.targetObject, iterator.name);
            var foldoutAttr = field?.GetCustomAttributes(typeof(FoldoutAttribute), true)
                                    .FirstOrDefault() as FoldoutAttribute;

            if (foldoutAttr != null)
            {
                if (!groups.ContainsKey(foldoutAttr.GroupName))
                {
                    groups[foldoutAttr.GroupName] = new List<SerializedProperty>();
                    groupOrder.Add(foldoutAttr.GroupName);
                }
                groups[foldoutAttr.GroupName].Add(iterator.Copy());
            }
            else
            {
                ungrouped.Add(iterator.Copy());
            }
        }

        foreach (var prop in ungrouped)
        {
            EditorGUILayout.PropertyField(prop, true);
        }

        foreach (var groupName in groupOrder)
        {
            string key = GetKey(serializedObject.targetObject, groupName);
            bool expanded = SessionState.GetBool(key, true);

            EditorGUILayout.Space(2);
            bool newExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(expanded, groupName);
            if (newExpanded != expanded)
            {
                SessionState.SetBool(key, newExpanded);
            }

            if (newExpanded)
            {
                EditorGUI.indentLevel++;
                foreach (var prop in groups[groupName])
                {
                    EditorGUILayout.PropertyField(prop, true);
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        serializedObject.ApplyModifiedProperties();
    }

    public static bool HasFoldoutFields(Object obj)
    {
        FieldInfo[] fields = obj.GetType().GetFields(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var f in fields)
        {
            if (f.IsDefined(typeof(FoldoutAttribute), true))
                return true;
        }
        return false;
    }

    private static FieldInfo GetFieldInfo(Object target, string fieldName)
    {
        return target.GetType().GetField(fieldName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }

    private static string GetKey(Object target, string groupName)
    {
        return $"FoldoutGroup_{target.GetEntityId()}_{groupName}";
    }
}
