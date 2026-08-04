using UnityEngine;
using UnityEditor;

namespace Equipment
{
    [CustomEditor(typeof(EquipmentSO))]
    public class EquipmentSOEditor : Editor
    {
        public static Color CommonColor = Color.white;
        public static Color UncommonColor = Color.green;
        public static Color RareColor = Color.blue;
        public static Color EpicColor = new Color(0.64f, 0.21f, 0.93f);
        public static Color LegendaryColor = new Color(1f, 0.5f, 0f);

        public override void OnInspectorGUI()
        {
            EquipmentSO equipmentSO = (EquipmentSO)target;

            base.OnInspectorGUI();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel("Total Gold Value");

            EditorGUILayout.LabelField(equipmentSO.goldCost.ToString());

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel("Rarity");

            GUIStyle gUIStyle = new GUIStyle(EditorStyles.boldLabel);
            switch (equipmentSO.Rarity)
            {
                case Rarity.Common:
                    gUIStyle.normal.textColor = CommonColor;
                    break;
                case Rarity.Uncommon:
                    gUIStyle.normal.textColor = UncommonColor;
                    break;
                case Rarity.Rare:
                    gUIStyle.normal.textColor = RareColor;
                    break;
                case Rarity.Epic:
                    gUIStyle.normal.textColor = EpicColor;
                    break;
                case Rarity.Legendary:
                    gUIStyle.normal.textColor = LegendaryColor;
                    break;
                default:
                    break;
            }

            EditorGUILayout.LabelField(equipmentSO.Rarity.ToString(), gUIStyle);

            EditorGUILayout.EndHorizontal();
        }
    }
}

