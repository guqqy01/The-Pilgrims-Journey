using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
[CustomPropertyDrawer(typeof(TabReference))]
public class TabReferencePropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    private string GetDropDownString(Tab currentTab, GameObject currentSubTab)
    {
        if (currentTab == null)
            return "Null";

        if (currentSubTab == null)
            return currentTab.gameObject.name;

        return $"{currentTab.gameObject.name}/{currentSubTab.name}";
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty tabProperty = property.FindPropertyRelative("tab");
        SerializedProperty subTabIndexProperty = property.FindPropertyRelative("subTabIndex");

        position = EditorGUI.PrefixLabel(position, label);

        Tab currentTab = tabProperty.objectReferenceValue as Tab;
        int currentSubTabIndex = subTabIndexProperty.intValue;
        GameObject currentSubTab = (currentSubTabIndex == -1 && currentTab != null) ? null : currentTab.subTabs[currentSubTabIndex];
        string dropdownString = GetDropDownString(currentTab, currentSubTab);


        if (EditorGUI.DropdownButton(position, new GUIContent(dropdownString), FocusType.Keyboard))
        {
            Tab[] tabs = Object.FindObjectsByType<Tab>(FindObjectsInactive.Include);

            GenericMenu menu = new GenericMenu();

            foreach (Tab tab in tabs)
            {
                if (tab.subTabs != null && tab.subTabs.Count > 0)
                {
                    foreach(GameObject subTab in tab.subTabs)
                    {
                        if (subTab == null)
                            continue;

                        string name = $"{tab.gameObject.name}/{subTab.name}";
                        menu.AddItem(new GUIContent(name), false, () => Setup(tabProperty, subTabIndexProperty, tab, subTab)); 
                    }
                }
                else
                {
                    string name = tab.gameObject.name;
                    menu.AddItem(new GUIContent(name), false, () => Setup(tabProperty, subTabIndexProperty, tab));
                }
            }

            menu.DropDown(position);
        }

        
    }

    void Setup(SerializedProperty tabProperty, SerializedProperty subTabIndexProperty, Tab tab)
    {
        tabProperty.objectReferenceValue = tab;
        subTabIndexProperty.intValue = -1;
        tabProperty.serializedObject.ApplyModifiedProperties();
    }

    void Setup(SerializedProperty tabProperty, SerializedProperty subTabIndexProperty, Tab tab, GameObject subTab)
    {
        tabProperty.objectReferenceValue = tab;
        subTabIndexProperty.intValue = tab.subTabs.IndexOf(subTab);
        tabProperty.serializedObject.ApplyModifiedProperties();
    }
}