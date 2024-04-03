using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;


[CustomPropertyDrawer(typeof(AbstractSelectorAttribute))]
public class AbstractSelectorAttributeDrawer : PropertyDrawer
{
    private SerializedProperty serializedClass;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.managedReferenceValue == null || !property.isExpanded)
            return 20;

        float height = 20;

        List<string> names = GetFieldsInManagedReferenceType(property.type);

        var nextProp = property.Copy();
        nextProp.NextVisible(false);

        while (property.NextVisible(true))
        {
            if (names.Contains(property.name))
                height += EditorGUI.GetPropertyHeight(property) + 2;

            if (SerializedProperty.EqualContents(property, nextProp))
                break;
        }

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

        serializedClass = property.Copy();
        SerializedObject obj = serializedClass.serializedObject;

        float yMin = position.yMin;

        string name = (property.managedReferenceValue != null) ? property.managedReferenceFullTypename.Split(" ").Last() : "None";
        if (name.Contains("."))
            name = name.Split(".").Last();

        float buttonHeight = 18;
        Rect dropDownRect = new Rect(position) { yMin = yMin, height = buttonHeight };
        yMin += buttonHeight + 2;

        property.isExpanded = EditorGUI.Foldout(dropDownRect, property.isExpanded, GUIContent.none);

        string serializedClassName = serializedClass.managedReferenceFieldTypename.Split(" ").Last();
        if (serializedClassName.Contains("."))
            serializedClassName = serializedClassName.Split(".").Last();

        dropDownRect = EditorGUI.PrefixLabel(dropDownRect, new GUIContent(serializedClassName));


        if (EditorGUI.DropdownButton(dropDownRect, new GUIContent(name), FocusType.Passive))
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("None"), property == null, () =>
            {
                EditorGUI.BeginChangeCheck();
                obj.UpdateIfRequiredOrScript();
                serializedClass.managedReferenceValue = null;
                obj.ApplyModifiedProperties();
                EditorGUI.EndChangeCheck();
            }
            );

            List<Type> types = FindDerivedTypes(Type.GetType(property.managedReferenceFieldTypename.Split(" ").Last())).ToList();
            foreach (Type type in types)
            {
                bool on = serializedClass.managedReferenceValue != null && Type.GetType(property.managedReferenceFullTypename.Split(" ").Last()) == type;
                menu.AddItem(new GUIContent(type.Name), on, () =>
                {
                    EditorGUI.BeginChangeCheck();
                    obj.UpdateIfRequiredOrScript();
                    serializedClass.managedReferenceValue = Activator.CreateInstance(type);
                    obj.ApplyModifiedProperties();
                    EditorGUI.EndChangeCheck();
                }
                );
            }

            menu.DropDown(dropDownRect);
        }

        if (property.managedReferenceValue == null)
            return;

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            List<string> names = GetFieldsInManagedReferenceType(property.type);

            var nextProp = property.Copy();
            nextProp.NextVisible(false);

            SerializedProperty iterator = property.Copy();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                if (names.Contains(iterator.name))
                    DrawProperty(position, iterator, ref yMin);

                if (SerializedProperty.EqualContents(iterator, nextProp))
                    break;

                enterChildren = false;
            }

            EditorGUI.indentLevel--;
        }
    }

    public IEnumerable<Type> FindDerivedTypes(Type baseType)
    {
        return Assembly.GetAssembly(baseType).GetTypes().Where(t => t != baseType && baseType.IsAssignableFrom(t));
    }

    private string GetStringBetweenArrowBrackets(string st)
    {
        int pFrom = st.IndexOf("<") + "<".Length;
        int pTo = st.LastIndexOf(">");

        return st.Substring(pFrom, pTo - pFrom);
    }

    private List<string> GetFieldsInManagedReferenceType(string type)
    {
        if (type.Contains("."))
            type = type.Split(".").Last();

        string result = GetStringBetweenArrowBrackets(type);

        FieldInfo[] fieldInfos = Type.GetType(result).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        List<string> names = new List<string>();

        for (int i = 0; i < fieldInfos.Length; i++)
        {
            names.Add(fieldInfos[i].Name);
        }

        return names;
    }

    private void DrawProperty(Rect position, SerializedProperty propertyToDraw, ref float yMin)
    {
        float propertyheight = EditorGUI.GetPropertyHeight(propertyToDraw);
        Rect radiusRect = new Rect(position) { yMin = yMin, height = propertyheight };

        EditorGUI.PropertyField(radiusRect, propertyToDraw);

        yMin += propertyheight + 2;
    }
}
