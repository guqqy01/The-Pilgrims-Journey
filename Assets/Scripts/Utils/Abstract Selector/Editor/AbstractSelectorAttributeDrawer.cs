/*using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;
using Unity.VisualScripting;

[CustomPropertyDrawer(typeof(AbstractSelectorAttribute))]
public class AbstractSelectorAttributeDrawer : PropertyDrawer
{
    private const string nullValueString = "Null";

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        AbstractSelectorAttribute selectorAttribute = (AbstractSelectorAttribute)attribute;

        SerializedObject obj = property.serializedObject;

        Rect dropDownRect = new Rect(position) { height = EditorGUIUtility.singleLineHeight };
        
        bool isNotNull = property.managedReferenceValue != null;

        //Check if property is expandable
        if (isNotNull && property.hasVisibleChildren)
        {
            property.isExpanded = EditorGUI.Foldout(dropDownRect, property.isExpanded, GUIContent.none);
        }

        dropDownRect = EditorGUI.PrefixLabel(dropDownRect, *//*new GUIContent(property.name == "data" ? " " : property.displayName)*//*new GUIContent(" "), property.prefabOverride ? EditorStyles.boldLabel : EditorStyles.label);

        string[] baseTypeAndAssemblyName = property.managedReferenceFieldTypename.Split(' ');
        string baseTypeString = $"{baseTypeAndAssemblyName[1]}, {baseTypeAndAssemblyName[0]}";
        Type baseType = Type.GetType(baseTypeString);

        string name = nullValueString;

        if (isNotNull)
        {
            string[] referenceTypeAndAssemblyName = property.managedReferenceFullTypename.Split(' ');
            string referenceTypeString = $"{referenceTypeAndAssemblyName[1]}, {referenceTypeAndAssemblyName[0]}";
            Type referenceType = Type.GetType(referenceTypeString);

            if (referenceType.HasAttribute(typeof(AbstractTitle)))
            {
                var attributes = referenceType.GetAttributes(typeof(AbstractTitle));

                foreach (AbstractTitle attribute in attributes)
                {
                    if (attribute.Type != null)
                    {
                        if (attribute.Type == baseType)
                        {
                            name = attribute.Title;
                            break;
                        }
                    }
                    else
                    {
                        name = attribute.Title;
                    }
                }
            }
            else
            {
                name = referenceTypeAndAssemblyName[1];
            }
        }

        if (name.Contains("."))
            name = name.Split(".").Last();


        //Button and generic menu to select abstract types
        if (EditorGUI.DropdownButton(dropDownRect, new GUIContent(name), FocusType.Passive))
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent(nullValueString), !isNotNull, () =>
            {
                EditorGUI.BeginChangeCheck();
                obj.UpdateIfRequiredOrScript();
                property.managedReferenceValue = null;
                obj.ApplyModifiedProperties();
                EditorGUI.EndChangeCheck();
            }
            );

            List<Type> types = FindDerivedTypes(baseType).ToList();

            foreach (Type type in types)
            {
                string title = null;
                if (type.HasAttribute(typeof(AbstractTitle)))
                {
                    var attributes = type.GetAttributes(typeof(AbstractTitle));

                    foreach (AbstractTitle attribute in attributes)
                    {
                        if (attribute.Type != null)
                        {
                            if (attribute.Type == baseType)
                            {
                                title = attribute.Title;
                                break;
                            }
                        }
                        else
                        {
                            title = attribute.Title;
                        }
                    }
                }

                bool on = isNotNull && property.managedReferenceFullTypename.Split(" ").Last() == type.ToString();

                menu.AddItem(new GUIContent(title != null ? title : type.Name), on, () =>
                {
                    EditorGUI.BeginChangeCheck();
                    obj.UpdateIfRequiredOrScript();
                    property.managedReferenceValue = Activator.CreateInstance(type);
                    obj.ApplyModifiedProperties();
                    EditorGUI.EndChangeCheck();
                }
                ); 
            }

            menu.DropDown(dropDownRect);
        }

        EditorGUI.PropertyField(position, property, true);
    }

    /// <summary>
    /// Finds all instantiable Types that are inherited from the baseType, also includes the baseType if it is instantiable
    /// </summary>
    /// <returns>the list of instantiable <see cref="Type"/>'s that are inherited from the baseType</returns>
    private List<Type> FindDerivedTypes(Type baseType)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        List<Type> derivedTypes = new List<Type>();

        if (CheckInstantiable(baseType))
        {
            derivedTypes.Add(baseType);
        }

        foreach (Assembly assembly in assemblies)
        {
            derivedTypes.AddRange(assembly.GetTypes().Where(t => t != baseType && baseType.IsAssignableFrom(t) && CheckInstantiable(t)));
        }

        return derivedTypes;
    }

    private List<Type> FindDerivedTypes(Type baseType, Type[] typeIncludes)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        List<Type> derivedTypes = new List<Type>();

        derivedTypes.AddRange(typeIncludes.Where(t => t != baseType && baseType.IsAssignableFrom(t) && CheckInstantiable(t)));

        return derivedTypes;
    }

    /// <summary>
    /// Checks to see if a type is instantiable
    /// </summary>
    /// <returns>Whether a type is instantiable</returns>
    private bool CheckInstantiable(Type type)
    {
        if (typeof(Component).IsAssignableFrom(type))
        {
            return false;
        }

        if (type == typeof(string) || type.IsValueType)
        {
            return true;
        }

        return !type.IsInterface && !type.IsGenericTypeDefinition && !type.IsAbstract && type.IsVisible;
    }
}
*/

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;

[CustomPropertyDrawer(typeof(AbstractSelectorAttribute))]
public class AbstractSelectorAttributeDrawer : PropertyDrawer
{
    private const string nullValueString = "Null";

    // --- CACHE DICTIONARIES ---
    private static readonly Dictionary<string, Type> BaseTypeCache = new();
    private static readonly Dictionary<Type, List<Type>> DerivedTypesCache = new();
    private static readonly Dictionary<Type, string> TypeTitleCache = new();
    private static readonly Dictionary<string, Type> TypeFromStringCache = new();

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        AbstractSelectorAttribute selectorAttribute = (AbstractSelectorAttribute)attribute;

        SerializedObject obj = property.serializedObject;

        Rect dropDownRect = new Rect(position) { height = EditorGUIUtility.singleLineHeight };

        bool isNotNull = property.managedReferenceValue != null;

        //Foldout logic (unchanged)
        if (isNotNull && property.hasVisibleChildren)
        {
            property.isExpanded = EditorGUI.Foldout(dropDownRect, property.isExpanded, GUIContent.none);
        }

        dropDownRect = EditorGUI.PrefixLabel(dropDownRect, new GUIContent(" "), property.prefabOverride ? EditorStyles.boldLabel : EditorStyles.label);

        // --- CACHED base type lookup ---
        Type baseType = GetBaseType(property.managedReferenceFieldTypename);

        string name = nullValueString;

        if (isNotNull)
        {
            Type referenceType = GetTypeFromString(property.managedReferenceFullTypename);
            name = GetDisplayNameForType(referenceType, baseType).AddSpacesBeforeCapsRegex();
        }

        if (name.Contains("."))
            name = name.Split('.').Last();

        // Dropdown button (unchanged UI logic)
        if (EditorGUI.DropdownButton(dropDownRect, new GUIContent(name), FocusType.Passive))
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent(nullValueString), !isNotNull, () =>
            {
                obj.UpdateIfRequiredOrScript();
                property.managedReferenceValue = null;
                obj.ApplyModifiedProperties();
            });

            foreach (Type type in GetDerivedTypes(baseType))
            {
                string title = GetDisplayNameForType(type, baseType).AddSpacesBeforeCapsRegex();
                bool on = isNotNull && property.managedReferenceFullTypename.Split(" ").Last() == type.ToString();

                menu.AddItem(new GUIContent(title), on, () =>
                {
                    obj.UpdateIfRequiredOrScript();
                    property.managedReferenceValue = Activator.CreateInstance(type);
                    obj.ApplyModifiedProperties();
                });
            }

            menu.DropDown(dropDownRect);
        }

        EditorGUI.PropertyField(position, property, true);
    }

    // ---------------- CACHED LOOKUPS ----------------

    private Type GetBaseType(string typename)
    {
        if (!BaseTypeCache.TryGetValue(typename, out Type type))
        {
            string[] split = typename.Split(' ');
            type = Type.GetType($"{split[1]}, {split[0]}");
            BaseTypeCache[typename] = type;
        }
        return type;
    }

    private Type GetTypeFromString(string typename)
    {
        if (!TypeFromStringCache.TryGetValue(typename, out Type type))
        {
            string[] split = typename.Split(' ');
            type = Type.GetType($"{split[1]}, {split[0]}");
            TypeFromStringCache[typename] = type;
        }
        return type;
    }

    private List<Type> GetDerivedTypes(Type baseType)
    {
        if (!DerivedTypesCache.TryGetValue(baseType, out List<Type> types))
        {
            types = FindDerivedTypesUncached(baseType);
            DerivedTypesCache[baseType] = types;
        }
        return types;
    }

    private string GetDisplayNameForType(Type type, Type baseType)
    {
        if (!TypeTitleCache.TryGetValue(type, out string title))
        {
            title = TryGetAttributeTitle(type, baseType) ?? type.Name;
            TypeTitleCache[type] = title;
        }
        return title;
    }

    private string TryGetAttributeTitle(Type type, Type baseType)
    {
        if (!Attribute.IsDefined(type, typeof(AbstractTitle)))
            return null;

        AbstractTitle abstractTitle = type.GetCustomAttribute<AbstractTitle>();

        if (abstractTitle == null) return null;

        if (abstractTitle.Type == null || abstractTitle.Type == baseType)
            return abstractTitle.Title;
        
        return null;
    }

    // --- original method, unchanged but only called once per type now ---
    private List<Type> FindDerivedTypesUncached(Type baseType)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        List<Type> derived = new List<Type>();

        if (CheckInstantiable(baseType))
            derived.Add(baseType);

        foreach (Assembly assembly in assemblies)
        {
            derived.AddRange(
                assembly.GetTypes().Where(t => t != baseType && baseType.IsAssignableFrom(t) && CheckInstantiable(t))
            );
        }
        return derived;
    }

    private bool CheckInstantiable(Type type)
    {
        if (typeof(Component).IsAssignableFrom(type))
            return false;

        if (type == typeof(string) || type.IsValueType)
            return true;

        return !type.IsInterface && !type.IsGenericTypeDefinition && !type.IsAbstract && type.IsVisible;
    }
}
