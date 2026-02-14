#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
public static class EditorExtensions
{
    public static bool EqualContents(this SerializedProperty serializedProperty1, SerializedProperty serializedProperty2)
    {
        return SerializedProperty.EqualContents(serializedProperty1, serializedProperty2);
    }

    public static object GetTargetObjectOfProperty(this SerializedProperty prop)
    {
        if (prop == null) return null;

        var path = prop.propertyPath.Replace(".Array.data[", "[");
        object obj = prop.serializedObject.targetObject;
        var elements = path.Split('.');
        foreach (var element in elements)
        {
            if (element.Contains("["))
            {
                var elementName = element.Substring(0, element.IndexOf("["));
                var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                obj = GetValue_Imp(obj, elementName, index);
            }
            else
            {
                obj = GetValue_Imp(obj, element);
            }
        }
        return obj;
    }

    private static object GetValue_Imp(object source, string name)
    {
        if (source == null)
            return null;
        var type = source.GetType();

        while (type != null)
        {
            var f = type.GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (f != null)
                return f.GetValue(source);

            var p = type.GetProperty(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            if (p != null)
                return p.GetValue(source, null);

            type = type.BaseType;
        }
        return null;
    }

    private static object GetValue_Imp(object source, string name, int index)
    {
        var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
        if (enumerable == null) return null;
        var enm = enumerable.GetEnumerator();

        for (int i = 0; i <= index; i++)
        {
            if (!enm.MoveNext()) return null;
        }
        return enm.Current;
    }

    public static SerializedProperty FindParentProperty(this SerializedProperty serializedProperty)
    {
        var propertyPaths = serializedProperty.propertyPath.Split('.');
        if (propertyPaths.Length <= 1)
        {
            return default;
        }

        var parentSerializedProperty = serializedProperty.serializedObject.FindProperty(propertyPaths.First());
        for (var index = 1; index < propertyPaths.Length - 1; index++)
        {
            if (propertyPaths[index] == "Array")
            {
                if (index + 1 == propertyPaths.Length - 1)
                {
                    // reached the end
                    break;
                }
                if (propertyPaths.Length > index + 1 && System.Text.RegularExpressions.Regex.IsMatch(propertyPaths[index + 1], "^data\\[\\d+\\]$"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(propertyPaths[index + 1], "^data\\[(\\d+)\\]$");
                    var arrayIndex = int.Parse(match.Groups[1].Value);
                    parentSerializedProperty = parentSerializedProperty.GetArrayElementAtIndex(arrayIndex);
                    index++;
                }
            }
            else
            {
                parentSerializedProperty = parentSerializedProperty.FindPropertyRelative(propertyPaths[index]);
            }
        }

        return parentSerializedProperty;
    }
}
#endif
